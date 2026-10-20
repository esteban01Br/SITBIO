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
        // La validación vive en el repositorio: el demo compara en memoria y el
        // modo Api llama a POST auth/login (BCrypt + JWT, fase 2b).
        var resultado = _datos.IntentarAcceso(email, password, tenant?.Id ?? 0, out var usuario);
        if (resultado != LoginResultado.Correcto || usuario is null)
            return resultado;

        var institucion = tenant ?? _datos.Tenants.FirstOrDefault(t => t.Id == usuario.TenantId);
        if (institucion is null)
            return LoginResultado.TenantInvalido;

        TenantActual = institucion;
        // En modo demo persiste en memoria; en modo Api la API ya actualizó UltimoAcceso.
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
        // Modo Api: el token JWT pertenece a un solo tenant (ADSO-ARQ-01 §6);
        // para operar sobre otra institución hay que cerrar sesión y volver a entrar.
        if (!_datos.SoportaCambioTenant) return false;
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
