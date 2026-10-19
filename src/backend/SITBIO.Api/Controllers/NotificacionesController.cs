using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>Centro de notificaciones del tenant.</summary>
[ApiController]
[Route("api/v1/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly SITBIOContext _db;

    public NotificacionesController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Notificacion>>> Get()
    {
        var tenantId = User.TenantId();
        var notificaciones = await _db.Notificaciones.AsNoTracking()
            .Where(n => n.TenantId == tenantId)
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();
        return notificaciones;
    }

    [HttpPost]
    public async Task<ActionResult<Notificacion>> Post([FromBody] Notificacion notificacion)
    {
        notificacion.Id = 0;
        notificacion.TenantId = User.TenantId();
        if (notificacion.Fecha == default) notificacion.Fecha = DateTime.Now;
        _db.Notificaciones.Add(notificacion);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), notificacion);
    }
}
