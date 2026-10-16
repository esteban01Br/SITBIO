using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Nombre legible de un rol para los selectores de rol de la interfaz.</summary>
public record OpcionRol(RolSistema Rol, string Nombre)
{
    public override string ToString() => Nombre;
};

/// <summary>
/// Presentación compartida de los roles del sistema RBAC (DOC-01-FCP §6.1).
/// La usan Usuarios, UsuarioDetalle y Configuración para no duplicar textos.
/// </summary>
public static class RolesSistema
{
    public static string Nombre(RolSistema rol) => rol switch
    {
        RolSistema.Administrador => "Administrador de Sistema",
        RolSistema.CoordinadorBiomedico => "Ingeniero / Coordinador Biomédico",
        RolSistema.Tecnico => "Técnico de Mantenimiento",
        RolSistema.PersonalClinico => "Personal Clínico / Asistencial",
        RolSistema.Director => "Director / Gerente Hospitalario",
        RolSistema.Regulador => "Ente Regulador / Auditor",
        _ => rol.ToString()
    };

    public static string NombreCorto(RolSistema rol) => rol switch
    {
        RolSistema.Administrador => "Administrador",
        RolSistema.CoordinadorBiomedico => "Coordinador biomédico",
        RolSistema.Tecnico => "Técnico",
        RolSistema.PersonalClinico => "Personal clínico",
        RolSistema.Director => "Director",
        RolSistema.Regulador => "Regulador",
        _ => rol.ToString()
    };

    public static string Sigla(RolSistema rol) => rol switch
    {
        RolSistema.Administrador => "ADM",
        RolSistema.CoordinadorBiomedico => "CBM",
        RolSistema.Tecnico => "TEC",
        RolSistema.PersonalClinico => "PCL",
        RolSistema.Director => "DIR",
        RolSistema.Regulador => "REG",
        _ => "---"
    };

    /// <summary>Lista legible de los 6 roles, en el orden del enum.</summary>
    public static IReadOnlyList<OpcionRol> Lista { get; } =
        Enum.GetValues<RolSistema>().Select(r => new OpcionRol(r, Nombre(r))).ToList();
}

/// <summary>Formulario de alta de usuario: es el contenido del diálogo modal.</summary>
public partial class FormularioUsuario : ObservableObject
{
    [ObservableProperty] private string _nombres = string.Empty;
    [ObservableProperty] private string _apellidos = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _cargo = string.Empty;
    [ObservableProperty] private OpcionRol? _rolSeleccionado;

    public IReadOnlyList<OpcionRol> Roles => RolesSistema.Lista;
}

/// <summary>
/// Módulo de usuarios: padrón de cuentas por institución con alta, activación y
/// desactivación auditadas (CU de administración de usuarios, RBAC / OBJ-02).
/// </summary>
public partial class UsuariosViewModel : ObservableObject
{
    private const string TodosLosRoles = "Todos los roles";
    private const string TodosLosEstados = "Todos los estados";

    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    public ObservableCollection<Usuario> Usuarios { get; } = new();
    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<string> RolesFiltro { get; } = new();
    public ObservableCollection<string> EstadosFiltro { get; } = new();

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private string _rolFiltro = TodosLosRoles;
    [ObservableProperty] private string _estadoFiltro = TodosLosEstados;
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "usuarios en la institución";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>Solo los roles con nivel Total sobre Usuarios escriben en el padrón.</summary>
    public bool PuedeEditarUsuarios => _sesion.PuedeEditar(AreaAplicacion.Usuarios);

    public UsuariosViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;

        RolesFiltro.Add(TodosLosRoles);
        foreach (var rol in Enum.GetValues<RolSistema>())
            RolesFiltro.Add(RolesSistema.Nombre(rol));

        EstadosFiltro.Add(TodosLosEstados);
        EstadosFiltro.Add("Activos");
        EstadosFiltro.Add("Inactivos");

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Reconstruye los indicadores y vuelve a aplicar los filtros activos.</summary>
    public void Cargar()
    {
        var usuarios = _datos.UsuariosDe(TenantId).ToList();

        Kpis.Clear();
        var total = usuarios.Count;
        var activos = usuarios.Count(u => u.Activo);
        var inactivos = total - activos;
        var administradores = usuarios.Count(u => u.Rol == RolSistema.Administrador);
        var rolPrincipal = usuarios.GroupBy(u => u.Rol)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .FirstOrDefault();

        Kpis.Add(TarjetaKpi.Crear(
            "Usuarios de la institución",
            Formateo.Entero(total),
            "", "info",
            delta: $"{Formateo.Entero(administradores)} con acceso total",
            deltaIcono: "",
            leyenda: "cuentas registradas",
            ayuda: "Padrón de usuarios aislado por tenant: solo se cuentan las cuentas de la institución activa (RSK-03)."));

        Kpis.Add(TarjetaKpi.Crear(
            "Usuarios activos",
            Formateo.Entero(activos),
            "", activos == total ? "exito" : "advertencia",
            delta: inactivos > 0 ? $"{Formateo.Entero(inactivos)} inactivos" : "todos activos",
            deltaIcono: inactivos > 0 ? "" : "",
            leyenda: "con sesión habilitada",
            ayuda: "Cuentas habilitadas para iniciar sesión. Se desactivan desde la tabla y queda registrado en la bitácora."));

        Kpis.Add(TarjetaKpi.Crear(
            "Rol más frecuente",
            rolPrincipal is null ? "—" : RolesSistema.Sigla(rolPrincipal.Key),
            "", "acento",
            delta: rolPrincipal is null
                ? "sin usuarios"
                : $"{Formateo.Entero(rolPrincipal.Count())} de {Formateo.Entero(total)} usuarios",
            deltaIcono: "",
            leyenda: rolPrincipal is null ? string.Empty : RolesSistema.NombreCorto(rolPrincipal.Key),
            ayuda: "Rol con mayor número de cuentas en la institución; define el perfil predominante del equipo."));

        Kpis.Add(TarjetaKpi.Crear(
            "Administradores",
            Formateo.Entero(administradores),
            "", administradores > 1 ? "peligro" : "info",
            delta: administradores > 1 ? "revisar privilegios" : "único administrador",
            deltaIcono: administradores > 1 ? "" : "",
            leyenda: "nivel total",
            ayuda: "Cuentas con nivel Total sobre todas las áreas. Se recomienda mantener el mínimo indispensable (principio de mínimo privilegio)."));

        Contador = Formateo.Entero(total);
        NotaContador = "usuarios en la institución";

        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var usuarios = _datos.UsuariosDe(TenantId).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim();
            usuarios = usuarios.Where(u =>
                u.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                u.Cargo.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (RolFiltro != TodosLosRoles)
        {
            var rol = Enum.GetValues<RolSistema>()
                .FirstOrDefault(r => RolesSistema.Nombre(r) == RolFiltro);
            usuarios = usuarios.Where(u => u.Rol == rol);
        }

        if (EstadoFiltro == "Activos") usuarios = usuarios.Where(u => u.Activo);
        else if (EstadoFiltro == "Inactivos") usuarios = usuarios.Where(u => !u.Activo);

        Usuarios.Clear();
        foreach (var u in usuarios
                     .OrderBy(u => u.Apellidos, StringComparer.CurrentCultureIgnoreCase)
                     .ThenBy(u => u.Nombres, StringComparer.CurrentCultureIgnoreCase))
            Usuarios.Add(u);
    }

    partial void OnBusquedaChanged(string value) => AplicarFiltros();
    partial void OnRolFiltroChanged(string value) => AplicarFiltros();
    partial void OnEstadoFiltroChanged(string value) => AplicarFiltros();

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void LimpiarFiltros()
    {
        Busqueda = string.Empty;
        RolFiltro = TodosLosRoles;
        EstadoFiltro = TodosLosEstados;
        _toast.Informacion("Se restablecieron los filtros del padrón de usuarios.", "Filtros limpiados");
    }

    [RelayCommand]
    private void Ver(Usuario? usuario)
    {
        if (usuario is null) return;
        _navegacion.Navegar(Rutas.UsuarioDetalle, usuario.Id);
    }

    [RelayCommand]
    private void AlternarActivo(Usuario? usuario)
    {
        if (usuario is null) return;

        if (!PuedeEditarUsuarios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Usuarios.", "Acceso de solo lectura");
            return;
        }

        usuario.Activo = !usuario.Activo;

        _auditoria.Registrar(
            AccionAuditoria.Actualizacion,
            "Usuario",
            usuario.Email,
            usuario.Activo
                ? $"Activación de la cuenta de {usuario.NombreCompleto}"
                : $"Desactivación de la cuenta de {usuario.NombreCompleto}");

        _toast.Exito(
            usuario.Activo
                ? $"{usuario.NombreCompleto} ya puede iniciar sesión."
                : $"{usuario.NombreCompleto} quedó bloqueado; no podrá iniciar sesión.",
            usuario.Activo ? "Usuario activado" : "Usuario desactivado");

        Cargar();
    }

    [RelayCommand]
    private void Nuevo()
    {
        if (!PuedeEditarUsuarios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Usuarios.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioUsuario
        {
            RolSeleccionado = RolesSistema.Lista.FirstOrDefault(r => r.Rol == RolSistema.Tecnico)
        };
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (ContenidoDialogo is not FormularioUsuario form) return;

        if (string.IsNullOrWhiteSpace(form.Nombres) || string.IsNullOrWhiteSpace(form.Apellidos))
        {
            _toast.Advertencia("Ingrese los nombres y los apellidos del usuario.", "Datos incompletos");
            return;
        }

        if (string.IsNullOrWhiteSpace(form.Email) ||
            !form.Email.Contains('@') || !form.Email.Contains('.'))
        {
            _toast.Advertencia("Ingrese un correo institucional válido (ej. nombre@institucion.com).", "Correo inválido");
            return;
        }

        if (form.RolSeleccionado is null)
        {
            _toast.Advertencia("Seleccione el rol del usuario dentro de la matriz RBAC.", "Rol pendiente");
            return;
        }

        if (_datos.UsuariosDe(TenantId).Any(u =>
                u.Email.Equals(form.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            _toast.Error("Ya existe una cuenta con ese correo en esta institución.", "Correo duplicado");
            return;
        }

        var usuario = new Usuario
        {
            TenantId = TenantId,
            Nombres = form.Nombres.Trim(),
            Apellidos = form.Apellidos.Trim(),
            Email = form.Email.Trim(),
            Cargo = string.IsNullOrWhiteSpace(form.Cargo) ? "Por definir" : form.Cargo.Trim(),
            Rol = form.RolSeleccionado.Rol,
            Activo = true,
            FechaRegistro = DateTime.Now,
            UltimoAcceso = null,
            PrimerIngreso = true,
            // Clave demo provisional (OBJ-02: en producción se usa BCrypt + JWT).
            PasswordHash = "SITBIO2026!"
        };

        _datos.AgregarUsuario(usuario);

        _auditoria.Registrar(AccionAuditoria.Creacion, "Usuario", usuario.Email,
            $"Alta de {usuario.NombreCompleto} con rol {usuario.NombreRol}");

        _toast.Exito($"Se creó la cuenta de {usuario.NombreCompleto} con el rol {usuario.NombreRol}.", "Usuario creado");

        DialogoAbierto = false;
        ContenidoDialogo = null;
        Cargar();
    }

    [RelayCommand]
    private void Cancelar()
    {
        DialogoAbierto = false;
        ContenidoDialogo = null;
    }
}
