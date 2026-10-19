using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Auth;

/// <summary>Sección "Jwt" de appsettings.json.</summary>
public class OpcionesJwt
{
    public string Issuer { get; set; } = "SITBIO";
    public string Audience { get; set; } = "SITBIO-Desktop";
    public string Clave { get; set; } = string.Empty;
    public int MinutosValidez { get; set; } = 30;
}

/// <summary>
/// Emisión de tokens JWT (HS256, 30 minutos) con los claims definidos en
/// ADSO-ARQ-01 §6: id_usuario, tenant_id y rol.
/// </summary>
public class ServicioJwt
{
    private readonly OpcionesJwt _opciones;

    public ServicioJwt(OpcionesJwt opciones) => _opciones = opciones;

    public (string Token, DateTime ExpiraEn) Generar(Usuario usuario)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_opciones.MinutosValidez);

        var claims = new List<Claim>
        {
            new("id_usuario", usuario.Id.ToString()),
            new("tenant_id", usuario.TenantId.ToString()),
            new("rol", usuario.Rol.ToString()),
            new("nombre", $"{usuario.Nombres} {usuario.Apellidos}".Trim()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}

/// <summary>Lectura de los claims del usuario autenticado.</summary>
public static class ClaimsExtensions
{
    public static int TenantId(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue("tenant_id"), out var tenantId) ? tenantId : 0;

    public static int IdUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue("id_usuario"), out var id) ? id : 0;

    public static RolSistema Rol(this ClaimsPrincipal usuario) =>
        Enum.TryParse<RolSistema>(usuario.FindFirstValue("rol"), out var rol) ? rol : RolSistema.Tecnico;
}

/// <summary>
/// Restricción de rol (defensa en profundidad): el cliente ya aplica RBAC con
/// PermisosRbac; el servidor la revalida en cada operación de escritura.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequiereRolAttribute : AuthorizeAttribute
{
    public RequiereRolAttribute(params RolSistema[] rolesPermitidos)
    {
        Roles = string.Join(", ", rolesPermitidos.Select(r => r.ToString()));
    }
}
