using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Elemento visible de la navegación lateral.</summary>
public partial class NavItem : ObservableObject
{
    public required string Ruta { get; init; }
    public required string Titulo { get; init; }
    public required string Icono { get; init; }
    public required AreaAplicacion Area { get; init; }
    public required string Grupo { get; init; }
    public bool EnDesarrollo { get; init; }
    public int Orden { get; init; }

    /// <summary>Marca el elemento activo para resaltarlo en la barra lateral.</summary>
    [ObservableProperty] private bool _activo;
}

/// <summary>Sección del menú lateral (OPERACIÓN, ACTIVOS, …).</summary>
public class NavGrupo
{
    public required string Titulo { get; init; }
    public ObservableCollection<NavItem> Items { get; } = new();
}

/// <summary>
/// Contenedor de la aplicación: navegación lateral, barra superior, cambio de
/// institución (tenant), centro de notificaciones y cierre de sesión.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly ISesionService _sesion;
    private readonly INavegacionService _navegacion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;

    public ObservableCollection<NavGrupo> Menu { get; } = new();
    public ObservableCollection<Notificacion> Notificaciones { get; } = new();

    [ObservableProperty] private object? _vistaActual;
    [ObservableProperty] private string _rutaSeleccionada = Rutas.Dashboard;
    [ObservableProperty] private string _tituloModulo = "Panel de control";
    [ObservableProperty] private string _subtituloModulo = string.Empty;
    [ObservableProperty] private bool _menuAbierto = true;
    [ObservableProperty] private bool _notificacionesAbiertas;
    [ObservableProperty] private int _notificacionesNoLeidas;

    public Usuario? Usuario => _sesion.UsuarioActual;
    public Institucion? Institucion => _sesion.TenantActual;
    public string NombreUsuario => Usuario?.NombreCompleto ?? string.Empty;
    public string RolUsuario => Usuario?.NombreRol ?? string.Empty;
    public string InicialesUsuario => Usuario?.Iniciales ?? "??";
    public string CedulaUsuario => Usuario?.Cargo ?? string.Empty;
    public bool EsAdministrador => Usuario?.Rol == RolSistema.Administrador;
    public bool PuedeVerNotificaciones => true;

    public IReadOnlyList<Institucion> Tenants => _sesion.Tenants;

    public ShellViewModel(ISesionService sesion, INavegacionService navegacion,
        IRepositorioDatos datos, IAuditoriaService auditoria, IToastService toast)
    {
        _sesion = sesion;
        _navegacion = navegacion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;

        _navegacion.Navego += AlNavegar;
        _sesion.SesionCambiada += () =>
        {
            OnPropertyChanged(nameof(Usuario));
            OnPropertyChanged(nameof(Institucion));
            OnPropertyChanged(nameof(NombreUsuario));
            OnPropertyChanged(nameof(RolUsuario));
            OnPropertyChanged(nameof(InicialesUsuario));
            OnPropertyChanged(nameof(CedulaUsuario));
            OnPropertyChanged(nameof(EsAdministrador));
            ReconstruirMenu();
            CargarNotificaciones();
        };

        ReconstruirMenu();
        CargarNotificaciones();
        _navegacion.Navegar(Rutas.Dashboard);
    }

    private void ReconstruirMenu()
    {
        Menu.Clear();
        if (Usuario is null) return;

        const string op = "OPERACIÓN";
        const string act = "ACTIVOS";
        const string ana = "ANÁLISIS Y GESTIÓN";
        const string sis = "SISTEMA";

        var elementos = new List<NavItem>
        {
            new() { Ruta = Rutas.Dashboard, Titulo = "Panel de control", Icono = "\uE80F", Area = AreaAplicacion.Dashboard, Grupo = op, Orden = 1 },
            new() { Ruta = Rutas.Ordenes, Titulo = "Órdenes de trabajo", Icono = "\uE90F", Area = AreaAplicacion.Ordenes, Grupo = op, Orden = 2 },
            new() { Ruta = Rutas.Tecnicos, Titulo = "Asignación de técnicos", Icono = "\uE716", Area = AreaAplicacion.Tecnicos, Grupo = op, Orden = 3 },

            new() { Ruta = Rutas.Equipos, Titulo = "Equipos biomédicos", Icono = "\uE912", Area = AreaAplicacion.Equipos, Grupo = act, Orden = 4 },
            new() { Ruta = Rutas.Calibraciones, Titulo = "Calibraciones", Icono = "\uE916", Area = AreaAplicacion.Equipos, EnDesarrollo = true, Grupo = act, Orden = 5 },
            new() { Ruta = Rutas.Inventario, Titulo = "Inventario de repuestos", Icono = "\uE7B8", Area = AreaAplicacion.Equipos, EnDesarrollo = true, Grupo = act, Orden = 6 },

            new() { Ruta = Rutas.Reportes, Titulo = "Reportes e indicadores", Icono = "\uE9F9", Area = AreaAplicacion.Reportes, Grupo = ana, Orden = 7 },
            new() { Ruta = Rutas.Formularios, Titulo = "Formularios dinámicos", Icono = "\uE729", Area = AreaAplicacion.Formularios, Grupo = ana, Orden = 8 },
            new() { Ruta = Rutas.Usuarios, Titulo = "Usuarios y permisos", Icono = "\uE902", Area = AreaAplicacion.Usuarios, Grupo = ana, Orden = 9 },

            new() { Ruta = Rutas.Auditoria, Titulo = "Bitácora de auditoría", Icono = "\uE81C", Area = AreaAplicacion.Auditoria, Grupo = sis, Orden = 10 },
            new() { Ruta = Rutas.Configuracion, Titulo = "Configuración", Icono = "\uE713", Area = AreaAplicacion.Configuracion, Grupo = sis, Orden = 11 },
        };

        foreach (var grupo in elementos.OrderBy(e => e.Orden)
                                       .GroupBy(e => e.Grupo))
        {
            var visibles = grupo.Where(item =>
                PermisosRbac.PuedeVer(Usuario.Rol, item.Area) &&
                (_navegacion.PuedeNavegar(item.Ruta) || item.EnDesarrollo)).ToList();

            if (visibles.Count == 0) continue;

            var contenedor = new NavGrupo { Titulo = grupo.Key };
            foreach (var item in visibles) contenedor.Items.Add(item);
            Menu.Add(contenedor);
        }
    }

    private void CargarNotificaciones()
    {
        Notificaciones.Clear();
        if (Institucion is null) return;
        foreach (var n in _datos.NotificacionesDe(Institucion.Id)) Notificaciones.Add(n);
        NotificacionesNoLeidas = Notificaciones.Count;
    }

    private void AlNavegar()
    {
        VistaActual = _navegacion.VistaActual;
        RutaSeleccionada = _navegacion.RutaActual;

        foreach (var grupo in Menu)
            foreach (var item in grupo.Items)
                item.Activo = item.Ruta == _navegacion.RutaActual && !item.EnDesarrollo;

        var itemActivo = Menu.SelectMany(g => g.Items).FirstOrDefault(m => m.Ruta == _navegacion.RutaActual);
        TituloModulo = itemActivo?.Titulo ?? _navegacion.RutaActual switch
        {
            Rutas.EquipoDetalle => "Hoja de vida del equipo",
            Rutas.OrdenDetalle => "Detalle de la orden",
            Rutas.Intervencion => "Intervención técnica",
            Rutas.FormBuilder => "Constructor de formularios",
            Rutas.UsuarioDetalle => "Detalle del usuario",
            _ => "SITBIO"
        };
        SubtituloModulo = Institucion?.Nombre ?? string.Empty;
        OnPropertyChanged(nameof(RolUsuario));
    }

    [RelayCommand]
    private void Navegar(NavItem? item)
    {
        if (item is null) return;

        if (item.EnDesarrollo)
        {
            _toast.Advertencia("Este módulo se habilita en la próxima entrega del MVP.",
                $"{item.Titulo} · próximamente");
            NavegarRuta(item.Ruta);
            return;
        }

        NavegarRuta(item.Ruta);
    }

    public void NavegarRuta(string ruta, object? parametro = null)
        => _navegacion.Navegar(ruta, parametro);

    [RelayCommand]
    private void AlternarMenu() => MenuAbierto = !MenuAbierto;

    [RelayCommand]
    private void AlternarNotificaciones() => NotificacionesAbiertas = !NotificacionesAbiertas;

    [RelayCommand]
    private void MarcarLeidas()
    {
        NotificacionesNoLeidas = 0;
        NotificacionesAbiertas = false;
        _toast.Informacion("Todas las notificaciones fueron marcadas como leídas.");
    }

    [RelayCommand]
    private void CambiarInstitucion(Institucion? tenant)
    {
        if (tenant is null) return;
        if (_sesion.CambiarTenant(tenant.Id))
        {
            _auditoria.Registrar(AccionAuditoria.Consulta, "Institucion", tenant.Nombre,
                "Cambio de tenant activo");
            _toast.Informacion($"Ahora opera sobre {tenant.Nombre}", "Institución cambiada");
            CargarNotificaciones();
            _navegacion.Navegar(Rutas.Dashboard);
        }
    }

    [RelayCommand]
    private void CerrarSesion()
    {
        _auditoria.Registrar(AccionAuditoria.CierreSesion, "Sesion",
            Usuario?.Email ?? string.Empty, "Cierre de sesión desde el contenedor");
        // SesionCambiada repone la pantalla de acceso en el VM raíz.
        _sesion.CerrarSesion();
    }
}
