using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>
/// Intervenciones técnicas en campo. La colección completa migrará a MongoDB en la
/// fase 3 (ADR-002); por ahora vive en PostgreSQL como paso intermedio.
/// </summary>
[ApiController]
[Route("api/v1/intervenciones")]
[Authorize]
public class IntervencionesController : ControllerBase
{
    private readonly SITBIOContext _db;

    public IntervencionesController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Intervencion>>> Get()
    {
        var tenantId = User.TenantId();
        var intervenciones = await _db.Intervenciones.AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.FechaInicio)
            .ToListAsync();
        return intervenciones;
    }

    /// <summary>GET /api/v1/intervenciones/orden/{ordenId} — la intervención de una orden.</summary>
    [HttpGet("orden/{ordenId:int}")]
    public async Task<ActionResult<Intervencion>> DeOrden(int ordenId)
    {
        var tenantId = User.TenantId();
        var intervencion = await _db.Intervenciones.AsNoTracking()
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.OrdenId == ordenId);
        return intervencion is null ? NotFound() : intervencion;
    }

    /// <summary>POST /api/v1/intervenciones — registro inicial (alta del técnico en campo).</summary>
    [HttpPost]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico, RolSistema.Tecnico)]
    public async Task<ActionResult<Intervencion>> Post([FromBody] Intervencion intervencion)
    {
        intervencion.Id = 0;
        intervencion.TenantId = User.TenantId();
        if (intervencion.FechaInicio == default) intervencion.FechaInicio = DateTime.Now;
        _db.Intervenciones.Add(intervencion);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(DeOrden), new { ordenId = intervencion.OrdenId }, intervencion);
    }

    /// <summary>PUT /api/v1/intervenciones/{id} — avance del checklist, repuestos y firma.</summary>
    [HttpPut("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico, RolSistema.Tecnico)]
    public async Task<IActionResult> Put(int id, [FromBody] Intervencion intervencion)
    {
        var actual = await _db.Intervenciones
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == User.TenantId());
        if (actual is null) return NotFound();

        intervencion.Id = id;
        intervencion.TenantId = actual.TenantId;
        _db.Entry(actual).CurrentValues.SetValues(intervencion);
        actual.Repuestos = intervencion.Repuestos;
        actual.Evidencias = intervencion.Evidencias;
        await _db.SaveChangesAsync();
        return Ok(actual);
    }
}
