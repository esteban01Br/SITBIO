using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>Usuarios y roles (RBAC). La contraseña jamás sale ni entra por JSON.</summary>
[ApiController]
[Route("api/v1/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly SITBIOContext _db;

    public UsuariosController(SITBIOContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Usuario>>> Get()
    {
        var tenantId = User.TenantId();
        var usuarios = await _db.Usuarios.AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Apellidos)
            .ToListAsync();
        return usuarios;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> GetPorId(int id)
    {
        var usuario = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == User.TenantId());
        return usuario is null ? NotFound() : usuario;
    }

    /// <summary>POST /api/v1/usuarios — alta de usuario. Si no llega contraseña, usa SITBIO2026!.</summary>
    [HttpPost]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<ActionResult<Usuario>> Post([FromBody] Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Email) || string.IsNullOrWhiteSpace(usuario.Nombres))
            return BadRequest(new { mensaje = "Nombre y correo son obligatorios." });

        usuario.Id = 0;
        usuario.TenantId = User.TenantId();
        usuario.Email = usuario.Email.Trim().ToLowerInvariant();
        usuario.FechaRegistro = DateTime.Now;
        // PasswordHash tiene [JsonIgnore]: nunca llega del cliente. Clave provisional
        // hasta que la fase 2b exponga un DTO de creación con contraseña.
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword("SITBIO2026!");

        _db.Usuarios.Add(usuario);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { mensaje = $"El correo {usuario.Email} ya está registrado en la institución." });
        }

        return CreatedAtAction(nameof(GetPorId), new { id = usuario.Id }, usuario);
    }

    /// <summary>PUT /api/v1/usuarios/{id} — perfil y rol (no modifica la contraseña).</summary>
    [HttpPut("{id:int}")]
    [RequiereRol(RolSistema.Administrador, RolSistema.CoordinadorBiomedico)]
    public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
    {
        var actual = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == User.TenantId());
        if (actual is null) return NotFound();

        var hash = actual.PasswordHash;
        usuario.Id = id;
        usuario.TenantId = actual.TenantId;
        usuario.Email = string.IsNullOrWhiteSpace(usuario.Email) ? actual.Email : usuario.Email.Trim().ToLowerInvariant();
        _db.Entry(actual).CurrentValues.SetValues(usuario);
        actual.PasswordHash = hash; // preservado: el payload no lo trae ([JsonIgnore])

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                           pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { mensaje = $"El correo {usuario.Email} ya está registrado en la institución." });
        }

        return Ok(actual);
    }
}
