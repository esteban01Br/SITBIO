using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Fila de la tabla resumen: indicador, valor actual, unidad y meta de gestión.</summary>
public record FilaIndicador(string Indicador, string Valor, string Unidad, string Meta);

/// <summary>
/// Módulo de reportes (OBJ-06 / §4.3 del DOC-01-FCP): indicadores del periodo,
/// gráficas de mantenimiento, resumen frente a metas y las cuatro exportaciones
/// Excel/PDF. Cada archivo lo genera y lo registra en la bitácora el servicio de
/// exportación, listo para auditoría INVIMA conforme a la Resolución 3100 de 2019.
/// </summary>
public partial class ReportesViewModel : ObservableObject
{
    private const string Periodo12 = "Últimos 12 meses";
    private const string Periodo6 = "Últimos 6 meses";
    private const string Periodo30 = "Últimos 30 días";

    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<PuntoSerie> Mantenimientos { get; } = new();
    public ObservableCollection<SegmentoDistribucion> PorEstado { get; } = new();
    public ObservableCollection<SegmentoDistribucion> PorTipo { get; } = new();
    public ObservableCollection<PuntoSerie> CumplimientoMensual { get; } = new();
    public ObservableCollection<FilaIndicador> Resumen { get; } = new();
    public ObservableCollection<OrdenTrabajo> OrdenesRecientes { get; } = new();
    public ObservableCollection<string> Periodos { get; } = new();

    [ObservableProperty] private string _periodoSeleccionado = Periodo12;
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "órdenes registradas en la institución";
    [ObservableProperty] private string _subtitulo = string.Empty;

    /// <summary>Solo los roles con acceso al área de Reportes generan exportaciones.</summary>
    public bool PuedeExportar => _sesion.IntentarAcceso(AreaAplicacion.Reportes);

    public ReportesViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;

        Periodos.Add(Periodo12);
        Periodos.Add(Periodo6);
        Periodos.Add(Periodo30);

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Meses que abarca el periodo seleccionado (30 días ⇒ el mes en curso).</summary>
    private int MesesPeriodo => PeriodoSeleccionado == Periodo6 ? 6
        : PeriodoSeleccionado == Periodo30 ? 1
        : 12;

    /// <summary>Fecha límite inferior del periodo para filtrar las órdenes exportables.</summary>
    private DateTime InicioPeriodo => PeriodoSeleccionado == Periodo6 ? DateTime.Today.AddMonths(-6)
        : PeriodoSeleccionado == Periodo30 ? DateTime.Today.AddDays(-30)
        : DateTime.Today.AddMonths(-12);

    /// <summary>Reconstruye indicadores, series, resumen y últimas órdenes del periodo.</summary>
    public void Cargar()
    {
        Kpis.Clear();
        Mantenimientos.Clear();
        PorEstado.Clear();
        PorTipo.Clear();
        CumplimientoMensual.Clear();
        Resumen.Clear();
        OrdenesRecientes.Clear();

        var periodo = string.IsNullOrWhiteSpace(PeriodoSeleccionado) ? Periodo12 : PeriodoSeleccionado.ToLower();
        var ind = _datos.Indicadores(TenantId);
        var ordenes = _datos.OrdenesDe(TenantId);

        Subtitulo = $"Indicadores de mantenimiento, costos y cumplimiento · {periodo}";
        Contador = Formateo.Entero(ordenes.Count);
        NotaContador = "órdenes registradas en la institución";

        // --------------------------- Tarjetas de indicadores ---------------------------
        Kpis.Add(TarjetaKpi.Crear(
            "Disponibilidad del parque",
            Formateo.Num(ind.DisponibilidadPorcentaje, "0.0"),
            "\uE912",
            ind.DisponibilidadPorcentaje >= 95 ? "exito" : ind.DisponibilidadPorcentaje >= 85 ? "info" : "advertencia",
            delta: $"{Formateo.Entero(ind.EquiposOperativos)} de {Formateo.Entero(ind.EquiposRegistrados)} operativos",
            deltaIcono: "\uE73E",
            leyenda: "meta ≥ 95,0 %",
            ayuda: "Equipos en servicio frente al total registrado; indicador clave del DOC-01-FCP (OBJ-04).",
            unidad: "%"));

        Kpis.Add(TarjetaKpi.Crear(
            "Cumplimiento de SLA",
            Formateo.Num(ind.CumplimientoSLA, "0.0"),
            "\uE73E",
            ind.CumplimientoSLA >= 90 ? "exito" : ind.CumplimientoSLA >= 80 ? "advertencia" : "peligro",
            delta: $"{Formateo.Entero(ind.OrdenesAbiertas)} órdenes abiertas",
            deltaIcono: ind.CumplimientoSLA >= 90 ? "\uE74A" : "\uE74B",
            leyenda: "meta ≥ 90,0 %",
            ayuda: "Porcentaje de órdenes atendidas dentro del plazo de respuesta comprometido.",
            unidad: "%"));

        Kpis.Add(TarjetaKpi.Crear(
            "MTBF (tiempo medio entre fallas)",
            Formateo.Num(ind.MtbfHoras, "0.#"),
            "\uE823",
            ind.MtbfHoras >= 350 ? "exito" : "advertencia",
            delta: ind.MtbfHoras >= 350 ? "meta ≥ 350 h alcanzada" : "meta ≥ 350 h",
            deltaIcono: ind.MtbfHoras >= 350 ? "\uE74A" : "\uE74B",
            leyenda: "horas de operación",
            ayuda: "Horas promedio de funcionamiento entre fallas del parque biomédico de la institución.",
            unidad: "h"));

        Kpis.Add(TarjetaKpi.Crear(
            "MTTR (tiempo medio de reparación)",
            Formateo.Num(ind.MttrHoras, "0.#"),
            "\uE77F",
            ind.MttrHoras <= 6 ? "exito" : ind.MttrHoras <= 10 ? "advertencia" : "peligro",
            delta: ind.MttrHoras <= 6 ? "meta ≤ 6,0 h cumplida" : "meta ≤ 6,0 h",
            deltaIcono: ind.MttrHoras <= 6 ? "\uE74A" : "\uE74B",
            leyenda: "horas de reparación",
            ayuda: "Tiempo promedio desde la solicitud hasta el cierre de la orden de trabajo.",
            unidad: "h"));

        Kpis.Add(TarjetaKpi.Crear(
            "Costo del mes",
            Formateo.Dinero(ind.CostoMes),
            "\uE8E5",
            ind.VariacionCosto > 10 ? "advertencia" : "info",
            delta: $"{(ind.VariacionCosto >= 0 ? "+" : "−")}{Formateo.Porcentaje(Math.Abs((double)ind.VariacionCosto))} vs. mes anterior",
            deltaIcono: ind.VariacionCosto >= 0 ? "\uE74A" : "\uE74B",
            leyenda: $"mes anterior {Formateo.Dinero(ind.CostoMesAnterior)}",
            ayuda: "Costos reales del mes; cuando una orden no tiene costo real se toma el estimado."));

        Kpis.Add(TarjetaKpi.Crear(
            "Preventivos vencidos",
            Formateo.Entero(ind.PreventivosVencidos),
            "\uE7BA",
            ind.PreventivosVencidos > 0 ? "advertencia" : "exito",
            delta: ind.PreventivosVencidos > 0 ? "requieren programación" : "todo al día",
            deltaIcono: ind.PreventivosVencidos > 0 ? "\uE7BA" : "\uE73E",
            leyenda: "meta: 0 vencidos",
            ayuda: "Equipos cuya fecha programada de mantenimiento preventivo ya venció."));

        // ------------------------------- Gráficas -------------------------------
        foreach (var punto in _datos.SerieMantenimientos(TenantId, MesesPeriodo)) Mantenimientos.Add(punto);
        foreach (var segmento in _datos.DistribucionPorEstado(TenantId)) PorEstado.Add(segmento);
        foreach (var segmento in _datos.DistribucionPorTipo(TenantId)) PorTipo.Add(segmento);
        foreach (var punto in SerieCumplimiento(ordenes)) CumplimientoMensual.Add(punto);

        // ------------------------- Tabla resumen de indicadores -------------------------
        Resumen.Add(new FilaIndicador("Disponibilidad del parque",
            Formateo.Num(ind.DisponibilidadPorcentaje, "0.0"), "%", "≥ 95,0 %"));
        Resumen.Add(new FilaIndicador("Cumplimiento de SLA",
            Formateo.Num(ind.CumplimientoSLA, "0.0"), "%", "≥ 90,0 %"));
        Resumen.Add(new FilaIndicador("MTBF (tiempo medio entre fallas)",
            Formateo.Num(ind.MtbfHoras, "0.#"), "horas", "≥ 350 h"));
        Resumen.Add(new FilaIndicador("MTTR (tiempo medio de reparación)",
            Formateo.Num(ind.MttrHoras, "0.#"), "horas", "≤ 6,0 h"));
        Resumen.Add(new FilaIndicador("Costo del mes",
            Formateo.Dinero(ind.CostoMes), "COP", $"≤ {Formateo.Dinero(ind.CostoMesAnterior)} (mes anterior)"));
        Resumen.Add(new FilaIndicador("Preventivos vencidos",
            Formateo.Entero(ind.PreventivosVencidos), "equipos", "0"));
        Resumen.Add(new FilaIndicador("Órdenes críticas abiertas",
            Formateo.Entero(ind.OrdenesCriticas), "órdenes", "0"));

        // -------------------------- Últimas 10 órdenes --------------------------
        foreach (var orden in ordenes.OrderByDescending(o => o.FechaSolicitud).Take(10))
            OrdenesRecientes.Add(orden);
    }

    /// <summary>Serie derivada: cumplimiento de SLA mes a mes según las órdenes del tenant.</summary>
    private IEnumerable<PuntoSerie> SerieCumplimiento(IEnumerable<OrdenTrabajo> ordenes)
    {
        var hoy = DateTime.Today;

        for (var i = MesesPeriodo - 1; i >= 0; i--)
        {
            var fecha = hoy.AddMonths(-i);
            var delMes = ordenes
                .Where(o => o.FechaSolicitud.Month == fecha.Month && o.FechaSolicitud.Year == fecha.Year)
                .ToList();

            var cumplimiento = delMes.Count == 0
                ? 100d
                : Math.Round((delMes.Count - delMes.Count(o => o.SLAVencido)) * 100.0 / delMes.Count, 1);

            yield return new PuntoSerie(PrimeraMayus(fecha.ToString("MMM")), cumplimiento);
        }
    }

    private static string PrimeraMayus(string texto) =>
        string.IsNullOrEmpty(texto) ? texto : char.ToUpperInvariant(texto[0]) + texto[1..];

    /// <summary>Órdenes del periodo seleccionado: son las que se exportan a Excel y PDF.</summary>
    private IEnumerable<OrdenTrabajo> OrdenesDelPeriodo() =>
        _datos.OrdenesDe(TenantId)
            .Where(o => o.FechaSolicitud >= InicioPeriodo)
            .OrderByDescending(o => o.FechaSolicitud)
            .ToList();

    /// <summary>Valida rol e institución antes de pedir el archivo al servicio.</summary>
    private bool ListoParaExportar()
    {
        if (!PuedeExportar)
        {
            _toast.Advertencia("Su rol no tiene acceso al área de Reportes para exportar.", "Acceso restringido");
            return false;
        }

        if (TenantId == 0)
        {
            _toast.Advertencia("No hay una institución activa; seleccione una antes de exportar.", "Sin institución");
            return false;
        }

        return true;
    }

    partial void OnPeriodoSeleccionadoChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        Cargar();
    }

    // ------------------------------- Comandos -------------------------------

    [RelayCommand]
    private void ExportarIndicadoresExcel()
    {
        if (!ListoParaExportar()) return;
        _exportacion.ExportarIndicadoresExcel(TenantId);
    }

    [RelayCommand]
    private void ExportarIndicadoresPdf()
    {
        if (!ListoParaExportar()) return;
        _exportacion.ExportarIndicadoresPdf(TenantId);
    }

    [RelayCommand]
    private void ExportarOrdenesExcel()
    {
        if (!ListoParaExportar()) return;
        var ordenes = OrdenesDelPeriodo().ToList();
        if (ordenes.Count == 0)
        {
            _toast.Advertencia("No hay órdenes registradas en el periodo seleccionado.", "Sin órdenes para exportar");
            return;
        }
        _exportacion.ExportarOrdenesExcel(TenantId, ordenes);
    }

    [RelayCommand]
    private void ExportarOrdenesPdf()
    {
        if (!ListoParaExportar()) return;
        var ordenes = OrdenesDelPeriodo().ToList();
        if (ordenes.Count == 0)
        {
            _toast.Advertencia("No hay órdenes registradas en el periodo seleccionado.", "Sin órdenes para exportar");
            return;
        }
        _exportacion.ExportarOrdenesPdf(TenantId, ordenes);
    }

    [RelayCommand]
    private void VerOrden(OrdenTrabajo? orden)
    {
        if (orden is null) return;
        _navegacion.Navegar(Rutas.OrdenDetalle, orden.Id);
    }
}
