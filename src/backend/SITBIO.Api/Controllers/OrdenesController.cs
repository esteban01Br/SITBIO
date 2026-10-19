using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>
/// Órdenes de trabajo: consulta, creación con código único (SIT-2026-000123) y
/// actualización de estado (kanban / línea de tiempo).
/// </summary>
[ApiController]
[Route("api/v1/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly SITBIOContext _db;

    public OrdenesController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<OrdenTrabajo>>> Get()
    {
        var tenantId = User.TenantId();
        var ordenes = await _db.Ordenes.AsNoTracking()
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.FechaSolicitud)
            .ToListAsync();
        return ordenes;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdenTrabajo>> GetPorId(int id)
    {
        var orden = await _db.Ordenes.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == User.TenantId());
        return orden is null ? NotFound() : orden;
    }

    /// <summary>GET /api/v1/ordenes/codigo/{codigo} — búsqueda por código único.</summary>
    [HttpGet("codigo/{codigo}")]
    public async Task<ActionResult<OrdenTrabajo>> GetPorCodigo(string codigo)
    {
        var tenantId = User.TenantId();
        var orden = await _db.Ordenes.AsNoTracking().FirstOrDefaultAsync(
            o => o.TenantId == tenantId && o.Codigo.ToLower() == codigo.ToLower());
        return orden is null ? NotFound() : orden;
    }

    /// <summary>GET /api/v1/ordenes/siguiente-codigo — genera SIT-{año}-{000001} del tenant.</summary>
    [HttpGet("siguiente-codigo")]
    public async Task<ActionResult<string>> SiguienteCodigo()
    {
        var tenantId = User.TenantId();
        var consecutivo = await _db.Ordenes.CountAsync(o => o.TenantId == tenantId) + 1;
        return Ok($"SIT-{DateTime.Today.Year}-{consecutivo:D6}");
    }

    /// <summary>POST /api/v1/ordenes — crea la orden (el servidor asigna el código si falta).</summary>
    [HttpPost]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico, RolSistema.PersonalClinico)]
    public async Task<ActionResult<OrdenTrabajo>> Post([FromBody] OrdenTrabajo orden)
    {
        if (string.IsNullOrWhiteSpace(orden.Titulo))
            return BadRequest(new { mensaje = "El título de la orden es obligatorio." });

        var tenantId = User.TenantId();
        orden.Id = 0;
        orden.TenantId = tenantId;

        if (string.IsNullOrWhiteSpace(orden.Codigo))
        {
            var consecutivo = await _db.Ordenes.CountAsync(o => o.TenantId == tenantId) + 1;
            orden.Codigo = $"SIT-{DateTime.Today.Year}-{consecutivo:D6}";
        }

        if (orden.SolicitanteId == 0)
        {
            orden.SolicitanteId = User.IdUsuario();
            orden.SolicitanteNombre = User.FindFirst("nombre")?.Value ?? string.Empty;
        }

        if (orden.FechaSolicitud == default) orden.FechaSolicitud = DateTime.Now;

        _db.Ordenes.Add(orden);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { mensaje = $"El código {orden.Codigo} ya existe en la institución." });
        }

        return CreatedAtAction(nameof(GetPorId), new { id = orden.Id }, orden);
    }

    /// <summary>
    /// PUT /api/v1/ordenes/{id} — actualización completa (estado, asignación, historial,
    /// comentarios y costos). La usa el cliente en cada cambio del flujo kanban.
    /// </summary>
    [HttpPut("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico, RolSistema.Tecnico)]
    public async Task<IActionResult> Put(int id, [FromBody] OrdenTrabajo orden)
    {
        var actual = await _db.Ordenes
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == User.TenantId());
        if (actual is null) return NotFound();

        orden.Id = id;
        orden.TenantId = actual.TenantId;
        _db.Entry(actual).CurrentValues.SetValues(orden);
        actual.Historial = orden.Historial;
        actual.Comentarios = orden.Comentarios;
        await _db.SaveChangesAsync();
        return Ok(actual);
    }
}
