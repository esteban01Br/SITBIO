using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>Perfil técnico: especialidades, disponibilidad y carga de trabajo.</summary>
[ApiController]
[Route("api/v1/tecnicos")]
[Authorize]
public class TecnicosController : ControllerBase
{
    private readonly SITBIOContext _db;

    public TecnicosController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Tecnico>>> Get()
    {
        var tenantId = User.TenantId();
        var tecnicos = await _db.Tecnicos.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
        return tecnicos;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tecnico>> GetPorId(int id)
    {
        var tecnico = await _db.Tecnicos.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == User.TenantId());
        return tecnico is null ? NotFound() : tecnico;
    }
}
