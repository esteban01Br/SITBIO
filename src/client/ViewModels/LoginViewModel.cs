using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Acceso de demostración para cada rol del sistema (RBAC).</summary>
public record AccesoDemo(string Rol, string Persona, string Cargo, string Email, string Contrasena, string Icono);

/// <summary>
/// Pantalla de acceso. En producción valida BCrypt + JWT (OBJ-02); por ahora
/// autentica contra los usuarios demo y abre la sesión con el tenant elegido.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly ISesionService _sesion;
    private readonly INavegacionService _navegacion;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;

    public ObservableCollection<Institucion> Tenants { get; }

    public IReadOnlyList<AccesoDemo> AccesosDemo { get; } = new List<AccesoDemo>
    {
        new("Administrador", "Laura Gómez", "Administradora de Sistema", "admin@sitbio.com", "Admin123!", "\uE713"),
        new("Coordinador Biomédico", "Andrés Montoya", "Ingeniero Biomédico", "coordinador@sitbio.com", "Coord123!", "\uE90F"),
        new("Técnico", "Camilo Restrepo", "Técnico Electrónica", "tecnico@sitbio.com", "Tec123!", "\uE77F"),
        new("Personal Clínico", "Valentina Ospina", "Jefe de Enfermería", "enfermera@sitbio.com", "Clini123!", "\uE716"),
        new("Director", "Roberto Zapata", "Director Ejecutivo", "director@sitbio.com", "Dir123!", "\uE9F9"),
        new("Regulador / Auditor", "Diana Caro", "Auditor Externo", "auditor@sitbio.com", "Audit123!", "\uEA18"),
    };

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _contrasena = string.Empty;
    [ObservableProperty] private Institucion? _institucion;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _procesando;
    [ObservableProperty] private bool _mostrarContrasena;

    [ObservableProperty] private string _emailEtiqueta = "Correo institucional";
    [ObservableProperty] private string _pieInferior = string.Empty;

    public LoginViewModel(ISesionService sesion, INavegacionService navegacion,
        IAuditoriaService auditoria, IToastService toast)
    {
        _sesion = sesion;
        _navegacion = navegacion;
        _auditoria = auditoria;
        _toast = toast;
        Tenants = new ObservableCollection<Institucion>(sesion.Tenants);
        Institucion = Tenants.FirstOrDefault();
        Email = "admin@sitbio.com";
        Contrasena = "Admin123!";
        PieInferior = $"SITBIO v1.0 · Plataforma híbrida web + escritorio · {Tenants.Count} instituciones conectadas";
    }

    partial void OnEmailChanged(string value)
    {
        if (Error is not null) Error = null;
        EmailEtiqueta = ProbarDemo(value) is AccesoDemo demo
            ? $"Correo institucional · {demo.Rol}"
            : "Correo institucional";
    }

    partial void OnContrasenaChanged(string value)
    {
        if (Error is not null) Error = null;
    }

    private AccesoDemo? ProbarDemo(string email)
        => AccesosDemo.FirstOrDefault(a => a.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Completa las credenciales del rol elegido en los accesos rápidos.</summary>
    [RelayCommand]
    private void AccesoRapido(AccesoDemo acceso)
    {
        Email = acceso.Email;
        Contrasena = acceso.Contrasena;
        Error = null;
    }

    [RelayCommand]
    private void AlternarContrasena() => MostrarContrasena = !MostrarContrasena;

    [RelayCommand]
    private async Task Ingresar()
    {
        if (Procesando) return;
        Error = null;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Contrasena))
        {
            Error = "Ingrese el correo y la contraseña para continuar.";
            return;
        }

        if (Institucion is null)
        {
            Error = "Seleccione la institución a la que desea acceder.";
            return;
        }

        Procesando = true;
        await Task.Delay(450); // UX: evita el parpadeo del proceso

        var resultado = _sesion.Login(Email, Contrasena, Institucion);

        if (resultado != LoginResultado.Correcto)
        {
            Procesando = false;
            Error = resultado switch
            {
                LoginResultado.CredencialesInvalidas => "Correo o contraseña incorrectos. Verifique los datos de acceso.",
                LoginResultado.UsuarioInactivo => "Este usuario está inactivo. Contacte al administrador del sistema.",
                LoginResultado.TenantInvalido => "No se pudo establecer la institución de origen.",
                _ => "No fue posible iniciar sesión."
            };
            return;
        }

        _auditoria.Registrar(AccionAuditoria.InicioSesion, "Sesion",
            _sesion.UsuarioActual?.Email ?? Email,
            $"Acceso a la plataforma con rol {_sesion.UsuarioActual?.NombreRol}");

        Procesando = false;
        _toast.Exito($"Bienvenido(a) {_sesion.UsuarioActual?.NombreCompleto}", "Sesión iniciada");
        // El cambio de sesión reemplaza la pantalla y Shell navega al panel inicial.
    }
}
