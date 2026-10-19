using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>
/// Bitácora de auditoría INMUTABLE (RSK-01): solo admite consulta (GET) y alta (POST).
/// No existen PUT ni DELETE a propósito — así lo exige la metodología.
/// </summary>
[ApiController]
[Route("api/v1/auditoria")]
[Authorize]
public class AuditoriaController : ControllerBase
{
    private readonly SITBIOContext _db;

    public AuditoriaController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<EventoAuditoria>>> Get()
    {
        var tenantId = User.TenantId();
        var eventos = await _db.Auditoria.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
        return eventos;
    }

    /// <summary>POST /api/v1/auditoria — el cliente registra aquí toda escritura (append-only).</summary>
    [HttpPost]
    public async Task<ActionResult<EventoAuditoria>> Post([FromBody] EventoAuditoria evento)
    {
        evento.Id = 0;
        evento.TenantId = User.TenantId();
        if (evento.Fecha == default) evento.Fecha = DateTime.Now;
        _db.Auditoria.Add(evento);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), evento);
    }
}
