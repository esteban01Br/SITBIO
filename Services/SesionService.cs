using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Services;

/// <summary>Estado de la sesión: usuario autenticado y tenant activo (multi-tenant).</summary>
public interface ISesionService
{
    Usuario? UsuarioActual { get; }
    Institucion? TenantActual { get; }
    bool Autenticado { get; }
    IReadOnlyList<Institucion> Tenants { get; }

    /// <summary>Se dispara al iniciar/cerrar sesión o al cambiar de tenant.</summary>
    event Action? SesionCambiada;

    bool IntentarAcceso(AreaAplicacion area);
    NivelAcceso Nivel(AreaAplicacion area);
    bool PuedeEditar(AreaAplicacion area);

    LoginResultado Login(string email, string password, Institucion? tenant);
    void CerrarSesion();
    bool CambiarTenant(int tenantId);
}

public enum LoginResultado
{
    Correcto,
    CredencialesInvalidas,
    UsuarioInactivo,
    TenantInvalido
}

public class SesionService : ISesionService
{
    private readonly IRepositorioDatos _datos;

    public SesionService(IRepositorioDatos datos) => _datos = datos;

    public Usuario? UsuarioActual { get; private set; }
    public Institucion? TenantActual { get; private set; }
    public bool Autenticado => UsuarioActual is not null;
    public IReadOnlyList<Institucion> Tenants => _datos.Tenants;
    public event Action? SesionCambiada;

    public LoginResultado Login(string email, string password, Institucion? tenant)
    {
        var usuario = _datos.Usuarios.FirstOrDefault(u =>
            u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (usuario is null)
            return LoginResultado.CredencialesInvalidas;

        // Credenciales demo. En producción: BCrypt + JWT (OBJ-02).
        if (!string.Equals(usuario.PasswordHash, password, StringComparison.Ordinal))
            return LoginResultado.CredencialesInvalidas;

        if (!usuario.Activo)
            return LoginResultado.UsuarioInactivo;

        var institucion = tenant ?? _datos.Tenants.FirstOrDefault(t => t.Id == usuario.TenantId);
        if (institucion is null)
            return LoginResultado.TenantInvalido;

        TenantActual = institucion;
        usuario.UltimoAcceso = DateTime.Now;
        usuario.PrimerIngreso = false;
        UsuarioActual = usuario;
        SesionCambiada?.Invoke();
        return LoginResultado.Correcto;
    }

    public void CerrarSesion()
    {
        UsuarioActual = null;
        TenantActual = null;
        SesionCambiada?.Invoke();
    }

    public bool CambiarTenant(int tenantId)
    {
        if (UsuarioActual is null) return false;
        // Un usuario solo puede operar dentro de su institución: aislamiento lógico (RSK-03).
        if (UsuarioActual.Rol != RolSistema.Administrador) return false;

        var tenant = _datos.Tenants.FirstOrDefault(t => t.Id == tenantId);
        if (tenant is null) return false;

        TenantActual = tenant;
        SesionCambiada?.Invoke();
        return true;
    }

    public NivelAcceso Nivel(AreaAplicacion area)
        => UsuarioActual is null ? NivelAcceso.Ninguno : PermisosRbac.Nivel(UsuarioActual.Rol, area);

    public bool IntentarAcceso(AreaAplicacion area) => Nivel(area) != NivelAcceso.Ninguno;

    public bool PuedeEditar(AreaAplicacion area) => Nivel(area) == NivelAcceso.Total;
}
