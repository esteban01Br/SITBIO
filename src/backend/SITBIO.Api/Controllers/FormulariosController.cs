using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>Formularios dinámicos definidos por el administrador del tenant (OBJ-06).</summary>
[ApiController]
[Route("api/v1/formularios")]
[Authorize]
public class FormulariosController : ControllerBase
{
    private readonly SITBIOContext _db;

    public FormulariosController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<FormularioDinamico>>> Get()
    {
        var tenantId = User.TenantId();
        var formularios = await _db.Formularios.AsNoTracking()
            .Where(f => f.TenantId == tenantId)
            .OrderBy(f => f.Nombre)
            .ToListAsync();
        return formularios;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FormularioDinamico>> GetPorId(int id)
    {
        var formulario = await _db.Formularios.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == User.TenantId());
        return formulario is null ? NotFound() : formulario;
    }

    [HttpPost]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<ActionResult<FormularioDinamico>> Post([FromBody] FormularioDinamico formulario)
    {
        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return BadRequest(new { mensaje = "El nombre del formulario es obligatorio." });

        formulario.Id = 0;
        formulario.TenantId = User.TenantId();
        if (formulario.FechaCreacion == default) formulario.FechaCreacion = DateTime.Now;
        _db.Formularios.Add(formulario);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = formulario.Id }, formulario);
    }

    [HttpPut("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<IActionResult> Put(int id, [FromBody] FormularioDinamico formulario)
    {
        var actual = await _db.Formularios
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == User.TenantId());
        if (actual is null) return NotFound();

        formulario.Id = id;
        formulario.TenantId = actual.TenantId;
        _db.Entry(actual).CurrentValues.SetValues(formulario);
        actual.Campos = formulario.Campos;
        await _db.SaveChangesAsync();
        return Ok(actual);
    }

    [HttpDelete("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<IActionResult> Delete(int id)
    {
        var formulario = await _db.Formularios
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == User.TenantId());
        if (formulario is null) return NotFound();

        _db.Formularios.Remove(formulario);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
