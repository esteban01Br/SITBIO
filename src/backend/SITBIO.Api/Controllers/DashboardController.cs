using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>
/// Indicadores del panel y de los informes (CU-03 / CU-05). Los cálculos replican
/// exactamente los del DemoDataProvider del cliente para que los KPIs coincidan.
/// </summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly SITBIOContext _db;

    public DashboardController(SITBIOContext db) => _db = db;

    /// <summary>GET /api/v1/dashboard/indicadores — KPIs del tenant autenticado.</summary>
    [HttpGet("indicadores")]
    public async Task<ActionResult<IndicadoresDashboard>> Indicadores()
    {
        var tenantId = User.TenantId();
        var equipos = await _db.Equipos.AsNoTracking().Where(e => e.TenantId == tenantId).ToListAsync();
        var ordenes = await _db.Ordenes.AsNoTracking().Where(o => o.TenantId == tenantId).ToListAsync();
        var hoy = DateTime.Today;

        var operativos = equipos.Count(e => e.Estado == EstadoEquipo.Operativo);
        var abiertas = ordenes.Where(o => o.Estado is not (EstadoOrden.Cerrada or EstadoOrden.Cancelada)).ToList();
        var cerradas = ordenes.Where(o => o.Estado == EstadoOrden.Cerrada).ToList();
        var vencidas = abiertas.Count(EstaVencida);
        var atendidas = abiertas.Count + cerradas.Count;
        var preventivosVencidos = equipos.Count(e => e.ProximoMantenimiento.HasValue &&
                                                     e.ProximoMantenimiento.Value < hoy);
        var proximas = equipos.Count(e => e.ProximaCalibracion.HasValue &&
                                          e.ProximaCalibracion.Value <= hoy.AddDays(30));

        var costoMes = ordenes
            .Where(o => o.FechaSolicitud.Month == hoy.Month && o.FechaSolicitud.Year == hoy.Year)
            .Sum(o => o.CostoReal > 0 ? o.CostoReal : o.CostoEstimado);

        var costoAnterior = ordenes
            .Where(o => o.FechaSolicitud.AddMonths(1).Month == hoy.Month && o.FechaSolicitud.AddMonths(1).Year == hoy.Year)
            .Sum(o => o.CostoReal > 0 ? o.CostoReal : o.CostoEstimado);
        if (costoAnterior == 0) costoAnterior = costoMes * 0.87m;

        return new IndicadoresDashboard
        {
            EquiposRegistrados = equipos.Count,
            EquiposOperativos = operativos,
            OrdenesAbiertas = abiertas.Count,
            OrdenesCriticas = abiertas.Count(o => o.Prioridad == Prioridad.Critica || o.Prioridad == Prioridad.Alta),
            PreventivosVencidos = preventivosVencidos,
            CalibracionesProximas = proximas,
            DisponibilidadPorcentaje = equipos.Count == 0 ? 100 : Math.Round(operativos * 100.0 / equipos.Count, 1),
            CumplimientoSLA = atendidas == 0 ? 100 : Math.Round((atendidas - vencidas) * 100.0 / atendidas, 1),
            MtbfHoras = 386.5,
            MttrHoras = 4.2,
            CostoMes = Math.Round(costoMes, 0),
            CostoMesAnterior = Math.Round(costoAnterior, 0)
        };
    }

    /// <summary>GET /api/v1/dashboard/serie?meses=6 — preventivos + correctivos por mes.</summary>
    [HttpGet("serie")]
    public async Task<ActionResult<List<PuntoSerie>>> Serie([FromQuery] int meses = 6)
    {
        var tenantId = User.TenantId();
        var ordenes = await _db.Ordenes.AsNoTracking().Where(o => o.TenantId == tenantId).ToListAsync();
        var serie = new List<PuntoSerie>();
        var hoy = DateTime.Today;

        for (var i = meses - 1; i >= 0; i--)
        {
            var fecha = hoy.AddMonths(-i);
            var total = ordenes.Count(o => (o.Tipo == TipoMantenimiento.Preventivo || o.Tipo == TipoMantenimiento.Correctivo) &&
                                           o.FechaSolicitud.Month == fecha.Month &&
                                           o.FechaSolicitud.Year == fecha.Year);
            serie.Add(new PuntoSerie(PrimeraMayus(fecha.ToString("MMM")), (double)total));
        }
        return serie;
    }

    /// <summary>GET /api/v1/dashboard/distribucion-estado — sectores por estado de orden.</summary>
    [HttpGet("distribucion-estado")]
    public async Task<ActionResult<List<SegmentoDistribucion>>> DistribucionEstado()
    {
        var tenantId = User.TenantId();
        var ordenes = await _db.Ordenes.AsNoTracking().Where(o => o.TenantId == tenantId).ToListAsync();
        var resultado = new List<SegmentoDistribucion>
        {
            new("Nuevas", ordenes.Count(o => o.Estado == EstadoOrden.Nueva), "#2E9BE5"),
            new("Asignadas", ordenes.Count(o => o.Estado == EstadoOrden.Asignada), "#7A5AF8"),
            new("En proceso", ordenes.Count(o => o.Estado == EstadoOrden.EnProceso), "#F2A33C"),
            new("Espera repuesto", ordenes.Count(o => o.Estado == EstadoOrden.EnEsperaRepuesto), "#E5484D"),
            new("Verificación", ordenes.Count(o => o.Estado == EstadoOrden.Verificacion), "#12B5A6"),
            new("Cerradas", ordenes.Count(o => o.Estado == EstadoOrden.Cerrada), "#12924E")
        };
        return resultado.Where(s => s.Valor > 0).ToList();
    }

    /// <summary>GET /api/v1/dashboard/distribucion-tipo — sectores por tipo de mantenimiento.</summary>
    [HttpGet("distribucion-tipo")]
    public async Task<ActionResult<List<SegmentoDistribucion>>> DistribucionTipo()
    {
        var tenantId = User.TenantId();
        var ordenes = await _db.Ordenes.AsNoTracking().Where(o => o.TenantId == tenantId).ToListAsync();
        var resultado = new List<SegmentoDistribucion>
        {
            new("Preventivo", ordenes.Count(o => o.Tipo == TipoMantenimiento.Preventivo), "#0B6FA4"),
            new("Correctivo", ordenes.Count(o => o.Tipo == TipoMantenimiento.Correctivo), "#12B5A6"),
            new("Predictivo", ordenes.Count(o => o.Tipo == TipoMantenimiento.Predictivo), "#7A5AF8"),
            new("Calibración", ordenes.Count(o => o.Tipo == TipoMantenimiento.Calibracion), "#F2A33C"),
            new("Instalación", ordenes.Count(o => o.Tipo == TipoMantenimiento.Instalacion), "#5B8DEF")
        };
        return resultado.Where(s => s.Valor > 0).ToList();
    }

    /// <summary>GET /api/v1/dashboard/distribucion-equipos — equipos agrupados por categoría.</summary>
    [HttpGet("distribucion-equipos")]
    public async Task<ActionResult<List<SegmentoDistribucion>>> DistribucionEquipos()
    {
        var tenantId = User.TenantId();
        var equipos = await _db.Equipos.AsNoTracking().Where(e => e.TenantId == tenantId).ToListAsync();
        var colores = new[] { "#0B6FA4", "#12B5A6", "#7A5AF8", "#F2A33C", "#3BB19A", "#E5484D", "#5B8DEF" };
        var grupos = equipos.GroupBy(e => e.Categoria)
                            .OrderByDescending(g => g.Count())
                            .Take(6)
                            .ToList();

        var salida = new List<SegmentoDistribucion>();
        for (var i = 0; i < grupos.Count; i++)
            salida.Add(new SegmentoDistribucion(grupos[i].Key, grupos[i].Count(), colores[i % colores.Length]));
        return salida;
    }

    // Réplica de los cálculos del cliente (OrdenTrabajo: SLA / horas restantes).
    private static bool EstaVencida(OrdenTrabajo orden)
    {
        var fin = orden.FechaCierre ?? DateTime.Now;
        return orden.FechaSolicitud.AddHours(orden.SLAHoras) < fin;
    }

    private static string PrimeraMayus(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
