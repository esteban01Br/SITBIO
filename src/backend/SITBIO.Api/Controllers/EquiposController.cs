using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>Inventario de equipos biomédicos: CRUD con aislamiento por tenant.</summary>
[ApiController]
[Route("api/v1/equipos")]
[Authorize]
public class EquiposController : ControllerBase
{
    private readonly SITBIOContext _db;

    public EquiposController(SITBIOContext db) => _db = db;

    /// <summary>GET /api/v1/equipos — equipos del tenant autenticado (SEC-003).</summary>
    [HttpGet]
    public async Task<ActionResult<List<Equipo>>> Get()
    {
        var tenantId = User.TenantId();
        var equipos = await _db.Equipos.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.CodigoInventario)
            .ToListAsync();
        return equipos;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Equipo>> GetPorId(int id)
    {
        var equipo = await _db.Equipos.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == User.TenantId());
        return equipo is null ? NotFound() : equipo;
    }

    /// <summary>POST /api/v1/equipos — alta de hoja de vida.</summary>
    [HttpPost]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<ActionResult<Equipo>> Post([FromBody] Equipo equipo)
    {
        if (string.IsNullOrWhiteSpace(equipo.Nombre))
            return BadRequest(new { mensaje = "El nombre del equipo es obligatorio." });

        equipo.Id = 0;
        equipo.TenantId = User.TenantId();
        _db.Equipos.Add(equipo);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { mensaje = "Ya existe un equipo con ese número de serie o código de inventario en la institución." });
        }

        return CreatedAtAction(nameof(GetPorId), new { id = equipo.Id }, equipo);
    }

    /// <summary>PUT /api/v1/equipos/{id} — actualización completa del equipo.</summary>
    [HttpPut("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<IActionResult> Put(int id, [FromBody] Equipo equipo)
    {
        var actual = await _db.Equipos
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == User.TenantId());
        if (actual is null) return NotFound();

        equipo.Id = id;
        equipo.TenantId = actual.TenantId;
        _db.Entry(actual).CurrentValues.SetValues(equipo);
        actual.CamposDinamicos = equipo.CamposDinamicos;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { mensaje = "Ya existe un equipo con ese número de serie o código de inventario en la institución." });
        }

        return Ok(actual);
    }

    /// <summary>DELETE /api/v1/equipos/{id} — retiro del inventario.</summary>
    [HttpDelete("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<IActionResult> Delete(int id)
    {
        var equipo = await _db.Equipos
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == User.TenantId());
        if (equipo is null) return NotFound();

        _db.Equipos.Remove(equipo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
