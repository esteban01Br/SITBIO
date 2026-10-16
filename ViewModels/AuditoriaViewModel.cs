using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>
/// Bitácora de auditoría (RSK-01): consulta, filtros y exportación de evidencia.
/// Es inmutable por diseño: este módulo no ofrece ninguna operación de
/// escritura, edición ni borrado sobre los eventos.
/// </summary>
public partial class AuditoriaViewModel : ObservableObject
{
    private const string TodasLasAcciones = "Todas las acciones";
    private const string RangoHoy = "Hoy";
    private const string Rango7 = "Últimos 7 días";
    private const string Rango30 = "Últimos 30 días";
    private const string RangoTodo = "Todo";

    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IExportacionService _exportacion;
    private readonly IToastService _toast;

    public ObservableCollection<EventoAuditoria> Eventos { get; } = new();
    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<string> AccionesFiltro { get; } = new();
    public ObservableCollection<string> RangosFiltro { get; } = new();

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private string _accionFiltro = TodasLasAcciones;
    [ObservableProperty] private string _rangoFiltro = RangoTodo;
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "eventos en la bitácora";

    public AuditoriaViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _toast = toast;
        _exportacion = exportacion;

        // La bitácora es de solo lectura: no se usa el servicio de auditoría para
        // escribir, y este módulo no navega; se reciben para respetar la firma.
        _ = auditoria;
        _ = navegacion;

        AccionesFiltro.Add(TodasLasAcciones);
        foreach (var accion in Enum.GetValues<AccionAuditoria>())
            AccionesFiltro.Add(NombreAccion(accion));

        RangosFiltro.Add(RangoHoy);
        RangosFiltro.Add(Rango7);
        RangosFiltro.Add(Rango30);
        RangosFiltro.Add(RangoTodo);

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Nombre visible de una acción, reutilizando el texto del modelo.</summary>
    private static string NombreAccion(AccionAuditoria accion) => new EventoAuditoria { Accion = accion }.AccionTexto;

    /// <summary>Recalcula los indicadores de la bitácora y reaplica los filtros.</summary>
    public void Cargar()
    {
        var eventos = _datos.AuditoriaDe(TenantId);

        var total = eventos.Count;
        var hoy = eventos.Count(e => e.Fecha.Date == DateTime.Today);
        var ultimos30 = eventos.Count(e => e.Fecha >= DateTime.Today.AddDays(-30));
        var usuariosDistintos = eventos.Select(e => e.Usuario).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var exportaciones = eventos.Count(e => e.Accion == AccionAuditoria.Exportacion);

        Kpis.Clear();

        Kpis.Add(TarjetaKpi.Crear(
            "Eventos en la bitácora",
            Formateo.Entero(total),
            "\uE81C", "info",
            delta: $"{Formateo.Entero(ultimos30)} en los últimos 30 días",
            deltaIcono: "\uE823",
            leyenda: "registros inmutables",
            ayuda: "La bitácora solo admite altas: nunca admite edición ni borrado (RSK-01), por lo que sirve como prueba ante auditorías."));

        Kpis.Add(TarjetaKpi.Crear(
            "Eventos de hoy",
            Formateo.Entero(hoy),
            "\uE787", hoy > 0 ? "exito" : "neutro",
            delta: hoy > 0 ? "actividad registrada" : "sin movimientos hoy",
            deltaIcono: hoy > 0 ? "\uE74A" : "\uE73E",
            leyenda: DateTime.Today.ToString("dd MMM yyyy"),
            ayuda: "Movimientos de la institución registrados desde la medianoche."));

        Kpis.Add(TarjetaKpi.Crear(
            "Usuarios distintos",
            Formateo.Entero(usuariosDistintos),
            "\uE902", "acento",
            delta: "cuentas con actividad",
            deltaIcono: "\uE74A",
            leyenda: "en la institución activa",
            ayuda: "Número de cuentas diferentes que aparecen en los eventos del tenant activo."));

        Kpis.Add(TarjetaKpi.Crear(
            "Exportaciones",
            Formateo.Entero(exportaciones),
            "\uE8E5", "info",
            delta: "evidencia descargable",
            deltaIcono: "\uE896",
            leyenda: "informes generados",
            ayuda: "Cada exportación de indicadores u órdenes deja un evento en la bitácora con el archivo entregado."));

        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var eventos = _datos.AuditoriaDe(TenantId).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim();
            eventos = eventos.Where(e =>
                e.Usuario.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Entidad.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Referencia.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Detalle.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (AccionFiltro != TodasLasAcciones)
        {
            var accion = Enum.GetValues<AccionAuditoria>()
                .FirstOrDefault(a => NombreAccion(a) == AccionFiltro);
            eventos = eventos.Where(e => e.Accion == accion);
        }

        var desde = RangoFiltro switch
        {
            RangoHoy => DateTime.Today,
            Rango7 => DateTime.Today.AddDays(-7),
            Rango30 => DateTime.Today.AddDays(-30),
            _ => DateTime.MinValue
        };
        eventos = eventos.Where(e => e.Fecha >= desde);

        Eventos.Clear();
        foreach (var evt in eventos.OrderByDescending(e => e.Fecha))
            Eventos.Add(evt);

        Contador = Formateo.Entero(Eventos.Count);
        NotaContador = "eventos listados";
    }

    partial void OnBusquedaChanged(string value) => AplicarFiltros();
    partial void OnAccionFiltroChanged(string value) => AplicarFiltros();
    partial void OnRangoFiltroChanged(string value) => AplicarFiltros();

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void Exportar()
    {
        if (TenantId == 0) return;

        // El servicio genera el PDF, registra el evento de exportación y avisa con toast.
        _exportacion.ExportarIndicadoresPdf(TenantId);
        Cargar();
    }

    [RelayCommand]
    private void LimpiarFiltros()
    {
        Busqueda = string.Empty;
        AccionFiltro = TodasLasAcciones;
        RangoFiltro = RangoTodo;
        _toast.Informacion("Se restablecieron los filtros de la bitácora.", "Filtros limpiados");
    }
}
