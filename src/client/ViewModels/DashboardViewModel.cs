using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>
/// Panel de control (CU-03 / CU-05 de la DOC-01-FCP): indicadores de
/// disponibilidad, carga de mantenimiento, órdenes prioritarias, recordatorios
/// de preventivos y calibraciones, y actividad reciente de auditoría.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<PuntoSerie> Mantenimientos { get; } = new();
    public ObservableCollection<SegmentoDistribucion> PorEstado { get; } = new();
    public ObservableCollection<SegmentoDistribucion> PorCategorias { get; } = new();
    public ObservableCollection<OrdenTrabajo> OrdenesPrioritarias { get; } = new();
    public ObservableCollection<Equipo> EquiposAtencion { get; } = new();
    public ObservableCollection<ActividadReciente> Actividad { get; } = new();

    [ObservableProperty] private string _saludo = "Panel de control";
    [ObservableProperty] private string _fecha = string.Empty;
    [ObservableProperty] private string _resumenOperativo = string.Empty;
    [ObservableProperty] private string _totalOrdenes = "0";
    [ObservableProperty] private string _totalEtiqueta = string.Empty;
    [ObservableProperty] private int _alertasActivas;

    public bool PuedeCrearOrden => _sesion.PuedeEditar(AreaAplicacion.Ordenes);
    public bool PuedeExportar => _sesion.IntentarAcceso(AreaAplicacion.Reportes) || _sesion.PuedeEditar(AreaAplicacion.Ordenes);

    public DashboardViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    public void Cargar()
    {
        Kpis.Clear();
        Mantenimientos.Clear();
        PorEstado.Clear();
        PorCategorias.Clear();
        OrdenesPrioritarias.Clear();
        EquiposAtencion.Clear();
        Actividad.Clear();

        var usuario = _sesion.UsuarioActual;
        Saludo = usuario is null ? "Panel de control" : $"Hola, {usuario.Nombres}";
        Fecha = Formateo.FechaHora(DateTime.Now);

        var ind = _datos.Indicadores(TenantId);

        // ---------------- Tarjetas de indicadores ----------------
        Kpis.Add(TarjetaKpi.Crear(
            "Parque biomédico operativo",
            $"{ind.EquiposOperativos}/{ind.EquiposRegistrados}",
            "", ind.DisponibilidadPorcentaje >= 95 ? "exito" : ind.DisponibilidadPorcentaje >= 85 ? "info" : "advertencia",
            delta: Formateo.Porcentaje(ind.DisponibilidadPorcentaje),
            deltaIcono: "",
            leyenda: "disponibilidad",
            ayuda: "Equipos operativos frente al total registrado. La disponibilidad es el indicador clave de la DOC-01-FCP (OBJ-04)."));

        Kpis.Add(TarjetaKpi.Crear(
            "Órdenes de trabajo abiertas",
            Formateo.Entero(ind.OrdenesAbiertas),
            "", ind.OrdenesCriticas > 0 ? "peligro" : "info",
            delta: ind.OrdenesCriticas > 0 ? $"{Formateo.Entero(ind.OrdenesCriticas)} críticas o altas" : "sin casos críticos",
            deltaIcono: ind.OrdenesCriticas > 0 ? "" : "",
            leyenda: "pendientes",
            ayuda: "Solicitudes en curso: nueva, asignada, en proceso, espera de repuesto y verificación."));

        Kpis.Add(TarjetaKpi.Crear(
            "Preventivos vencidos",
            Formateo.Entero(ind.PreventivosVencidos),
            "", ind.PreventivosVencidos > 0 ? "advertencia" : "exito",
            delta: ind.PreventivosVencidos > 0 ? "requiere programación" : "todo al día",
            deltaIcono: ind.PreventivosVencidos > 0 ? "" : "",
            leyenda: "fuera de fecha",
            ayuda: "Equipos cuya fecha programada de mantenimiento preventivo ya venció."));

        Kpis.Add(TarjetaKpi.Crear(
            "Cumplimiento de SLA",
            Formateo.Porcentaje(ind.CumplimientoSLA),
            "", ind.CumplimientoSLA >= 90 ? "exito" : ind.CumplimientoSLA >= 80 ? "advertencia" : "peligro",
            delta: $"MTTR {Formateo.Num(ind.MttrHoras, "0.0")} h",
            deltaIcono: "",
            leyenda: "últimos 30 días",
            ayuda: "Porcentaje de órdenes cerradas dentro del plazo comprometido (tiempo de respuesta SLA)."));

        TotalOrdenes = Formateo.Entero(ind.OrdenesAbiertas);
        TotalEtiqueta = "órdenes abiertas · " + Formateo.Porcentaje(ind.CumplimientoSLA) + " de cumplimiento";
        AlertasActivas = ind.PreventivosVencidos + ind.OrdenesCriticas + ind.CalibracionesProximas;

        ResumenOperativo =
            $"Disponibilidad del parque biomédico: {Formateo.Porcentaje(ind.DisponibilidadPorcentaje)} · " +
            $"MTBF {Formateo.Num(ind.MtbfHoras, "0.0")} h · MTTR {Formateo.Num(ind.MttrHoras, "0.0")} h · " +
            $"costo del mes {Formateo.Dinero(ind.CostoMes)}" +
            (ind.VariacionCosto != 0
                ? $" ({(ind.VariacionCosto > 0 ? "+" : "−")}{Formateo.Porcentaje((double)Math.Abs(ind.VariacionCosto))} frente al mes anterior)"
                : string.Empty);

        // ---------------- Gráficas ----------------
        foreach (var p in _datos.SerieMantenimientos(TenantId, 6)) Mantenimientos.Add(p);
        foreach (var s in _datos.DistribucionPorEstado(TenantId)) PorEstado.Add(s);
        foreach (var s in _datos.DistribucionEquiposPorCategoria(TenantId)) PorCategorias.Add(s);

        // ---------------- Órdenes prioritarias ----------------
        var prioritarias = _datos.OrdenesDe(TenantId)
            .Where(o => o.EstaAbierta)
            .OrderByDescending(o => o.Prioridad == Prioridad.Critica)
            .ThenByDescending(o => o.Prioridad == Prioridad.Alta)
            .ThenBy(o => o.HorasRestantes)
            .Take(7);

        foreach (var o in prioritarias) OrdenesPrioritarias.Add(o);

        // ---------------- Equipos que requieren atención ----------------
        foreach (var e in _datos.EquiposDe(TenantId)
                     .Where(e => e.PreventivoVencido || e.Estado is EstadoEquipo.FueraDeServicio or EstadoEquipo.EnMantenimiento)
                     .OrderByDescending(e => e.PreventivoVencido)
                     .ThenBy(e => e.ProximoMantenimiento)
                     .Take(7))
            EquiposAtencion.Add(e);

        // ---------------- Actividad reciente (bitácora) ----------------
        foreach (var evt in _datos.AuditoriaDe(TenantId).OrderByDescending(e => e.Fecha).Take(7))
        {
            Actividad.Add(new ActividadReciente
            {
                Fecha = evt.Fecha,
                Titulo = $"{evt.AccionTexto} · {evt.Entidad}",
                Descripcion = string.IsNullOrWhiteSpace(evt.Referencia) ? evt.Detalle : $"{evt.Referencia} — {evt.Detalle}",
                Usuario = $"{evt.Usuario} ({evt.Rol})",
                Icono = evt.Icono,
                Color = evt.Accion switch
                {
                    AccionAuditoria.Eliminacion => Paleta.PeligroHex,
                    AccionAuditoria.Creacion or AccionAuditoria.Actualizacion => Paleta.InfoHex,
                    AccionAuditoria.Exportacion => Paleta.AcentoHex,
                    _ => Paleta.NeutroHex
                }
            });
        }
    }

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void Actualizar()
    {
        Cargar();
        _toast.Informacion("Los indicadores se recalcularon con la información más reciente.", "Panel actualizado");
    }

    [RelayCommand]
    private void ExportarExcel()
    {
        if (TenantId == 0) return;
        _exportacion.ExportarIndicadoresExcel(TenantId);
    }

    [RelayCommand]
    private void ExportarPdf()
    {
        if (TenantId == 0) return;
        _exportacion.ExportarIndicadoresPdf(TenantId);
    }

    [RelayCommand]
    private void NuevaOrden() => _navegacion.Navegar(Rutas.Ordenes);

    [RelayCommand]
    private void VerOrden(OrdenTrabajo? orden)
    {
        if (orden is null) return;
        _navegacion.Navegar(Rutas.OrdenDetalle, orden.Id);
    }

    [RelayCommand]
    private void VerEquipo(Equipo? equipo)
    {
        if (equipo is null) return;
        _navegacion.Navegar(Rutas.EquipoDetalle, equipo.Id);
    }

    [RelayCommand]
    private void Ir(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta)) return;
        _navegacion.Navegar(ruta);
    }
}
