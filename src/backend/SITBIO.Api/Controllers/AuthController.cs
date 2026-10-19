using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Auth;
using SITBIO.Api.Data;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Controllers;

/// <summary>Autenticación: POST /api/v1/auth/login (BCrypt + JWT, ADSO-ARQ-01 §6).</summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly SITBIOContext _db;
    private readonly ServicioJwt _jwt;

    public AuthController(SITBIOContext db, ServicioJwt jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>Valida credenciales contra BCrypt y devuelve un token JWT de 30 minutos.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest peticion)
    {
        if (string.IsNullOrWhiteSpace(peticion.Email) || string.IsNullOrWhiteSpace(peticion.Clave))
            return BadRequest(new { mensaje = "El correo y la contraseña son obligatorios." });

        var email = peticion.Email.Trim().ToLowerInvariant();

        var consulta = _db.Usuarios.Where(u => u.Activo && u.Email.ToLower() == email);
        if (peticion.TenantId > 0)
            consulta = consulta.Where(u => u.TenantId == peticion.TenantId);

        var usuario = await consulta.FirstOrDefaultAsync();

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(peticion.Clave, usuario.PasswordHash))
            return Unauthorized(new { mensaje = "Credenciales inválidas para la institución seleccionada." });

        var registro = await _db.Usuarios.FirstAsync(u => u.Id == usuario.Id);
        registro.UltimoAcceso = DateTime.Now;
        await _db.SaveChangesAsync();

        var (token, expira) = _jwt.Generar(usuario);
        return Ok(new LoginResponse { Token = token, ExpiraEn = expira, Usuario = usuario });
    }
}

/// <summary>Instituciones activas: se consultan antes de autenticar (selector de tenant).</summary>
[ApiController]
[Route("api/v1/tenants")]
public class TenantsController : ControllerBase
{
    private readonly SITBIOContext _db;

    public TenantsController(SITBIOContext db) => _db = db;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Institucion>>> Get() =>
        await _db.Instituciones.AsNoTracking()
                 .Where(i => i.Estado != EstadoTenant.Suspendido)
                 .OrderBy(i => i.Id)
                 .ToListAsync();
}
