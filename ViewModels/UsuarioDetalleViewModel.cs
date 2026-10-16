using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Contenido del diálogo de cambio de rol de un usuario.</summary>
public partial class FormularioRol : ObservableObject
{
    [ObservableProperty] private OpcionRol? _rolSeleccionado;

    public IReadOnlyList<OpcionRol> Roles => RolesSistema.Lista;
}

/// <summary>Fila de la matriz RBAC: un área con el nivel efectivo del rol consultado.</summary>
public class FilaPermiso
{
    public AreaAplicacion Area { get; init; }
    public string AreaTexto { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public NivelAcceso Nivel { get; init; }
}

/// <summary>
/// Ficha de un usuario: perfil completo, matriz RBAC de su rol
/// (PermisosRbac.Nivel) y los últimos 15 eventos de bitácora que lo involucran.
/// </summary>
public partial class UsuarioDetalleViewModel : ObservableObject
{
    private readonly int _id;
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;

    private Usuario? _usuario;

    public ObservableCollection<FilaPermiso> Matriz { get; } = new();
    public ObservableCollection<EventoAuditoria> Historial { get; } = new();

    [ObservableProperty] private string _nombre = "Ficha de usuario";
    [ObservableProperty] private string _subtitulo = "Cargando la información de la cuenta…";
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "eventos auditados del usuario";
    [ObservableProperty] private string _iniciales = "?";
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _cargo = string.Empty;
    [ObservableProperty] private string _nombreRol = string.Empty;
    [ObservableProperty] private string _siglaRol = "---";
    [ObservableProperty] private bool _activo;
    [ObservableProperty] private bool _sinAcceso;
    [ObservableProperty] private string _fechaRegistro = "—";
    [ObservableProperty] private string _ultimoAcceso = "—";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>Solo los roles con nivel Total sobre Usuarios cambian roles.</summary>
    public bool PuedeEditarUsuarios => _sesion.PuedeEditar(AreaAplicacion.Usuarios);

    public UsuarioDetalleViewModel(int id, ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _id = id;
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;

        // La ficha no genera reportes propios: el servicio se recibe para
        // respetar la firma única de módulos de detalle del contrato.
        _ = exportacion;

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Carga perfil, matriz RBAC y historial de auditoría del usuario.</summary>
    private void Cargar()
    {
        _usuario = _datos.Usuario(_id);

        if (_usuario is null)
        {
            Nombre = "Usuario no disponible";
            Subtitulo = "La cuenta solicitada ya no existe en esta institución";
            return;
        }

        var u = _usuario;

        Nombre = u.NombreCompleto;
        Subtitulo = $"{u.Cargo} · {u.Email}";
        Iniciales = u.Iniciales;
        Email = u.Email;
        Cargo = u.Cargo;
        NombreRol = u.NombreRol;
        SiglaRol = u.SiglaRol;
        Activo = u.Activo;
        FechaRegistro = Formateo.FechaHora(u.FechaRegistro);
        SinAcceso = u.UltimoAcceso is null;
        UltimoAcceso = u.UltimoAcceso is null ? "—" : Formateo.FechaHora(u.UltimoAcceso.Value);

        // -------- Matriz RBAC del rol del usuario (DOC-01-FCP §6.1) --------
        Matriz.Clear();
        foreach (var area in Enum.GetValues<AreaAplicacion>())
        {
            Matriz.Add(new FilaPermiso
            {
                Area = area,
                AreaTexto = TituloArea(area),
                Descripcion = PropositoArea(area),
                Nivel = PermisosRbac.Nivel(u.Rol, area)
            });
        }

        // -------- Historial de auditoría del usuario (máx. 15 eventos) --------
        Historial.Clear();
        foreach (var evt in _datos.AuditoriaDe(TenantId)
                     .Where(e => e.Usuario == u.NombreCompleto)
                     .Take(15))
            Historial.Add(evt);

        Contador = Formateo.Entero(Historial.Count);
        NotaContador = "eventos auditados del usuario";
    }

    private static string TituloArea(AreaAplicacion area) => area switch
    {
        AreaAplicacion.Dashboard => "Panel de control",
        AreaAplicacion.Equipos => "Equipos biomédicos",
        AreaAplicacion.Ordenes => "Órdenes de trabajo",
        AreaAplicacion.Intervencion => "Intervenciones",
        AreaAplicacion.Tecnicos => "Técnicos",
        AreaAplicacion.Formularios => "Formularios dinámicos",
        AreaAplicacion.Reportes => "Reportes e informes",
        AreaAplicacion.Usuarios => "Usuarios y roles",
        AreaAplicacion.Configuracion => "Configuración",
        AreaAplicacion.Auditoria => "Auditoría",
        _ => area.ToString()
    };

    private static string PropositoArea(AreaAplicacion area) => area switch
    {
        AreaAplicacion.Dashboard => "Indicadores, alertas y actividad reciente",
        AreaAplicacion.Equipos => "Parque biomédico y hoja de vida",
        AreaAplicacion.Ordenes => "Solicitudes, SLA y flujo de trabajo",
        AreaAplicacion.Intervencion => "Checklists, repuestos y firmas",
        AreaAplicacion.Tecnicos => "Cuadrillas, especialidades y carga",
        AreaAplicacion.Formularios => "Plantillas dinámicas del tenant",
        AreaAplicacion.Reportes => "Indicadores exportables para auditoría",
        AreaAplicacion.Usuarios => "Padrón de cuentas y roles",
        AreaAplicacion.Configuracion => "Parámetros de la institución",
        AreaAplicacion.Auditoria => "Bitácora inmutable de eventos",
        _ => "Área del sistema"
    };

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void EditarRol()
    {
        if (_usuario is null) return;

        if (!PuedeEditarUsuarios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Usuarios.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioRol
        {
            RolSeleccionado = RolesSistema.Lista.FirstOrDefault(r => r.Rol == _usuario.Rol)
        };
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (_usuario is null || ContenidoDialogo is not FormularioRol form) return;

        if (form.RolSeleccionado is null)
        {
            _toast.Advertencia("Seleccione el nuevo rol dentro de la matriz RBAC.", "Rol pendiente");
            return;
        }

        if (form.RolSeleccionado.Rol == _usuario.Rol)
        {
            _toast.Informacion("La cuenta ya tiene el rol seleccionado.", "Sin cambios");
            DialogoAbierto = false;
            ContenidoDialogo = null;
            return;
        }

        var anterior = _usuario.NombreRol;
        var email = _usuario.Email;
        var nombre = _usuario.NombreCompleto;
        _usuario.Rol = form.RolSeleccionado.Rol;

        _auditoria.Registrar(AccionAuditoria.Actualizacion, "Usuario", email,
            $"Cambio de rol de {anterior} a {_usuario.NombreRol} para {nombre}");

        _toast.Exito($"{nombre} pasó a tener el rol {_usuario.NombreRol}.", "Rol actualizado");

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

    [RelayCommand]
    private void Volver() => _navegacion.Navegar(Rutas.Usuarios);
}
