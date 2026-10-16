using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Services;

/// <summary>Claves de navegación de las pantallas del sistema.</summary>
public static class Rutas
{
    public const string Login = "login";
    public const string Dashboard = "dashboard";
    public const string Equipos = "equipos";
    public const string EquipoDetalle = "equipo-detalle";
    public const string Ordenes = "ordenes";
    public const string OrdenDetalle = "orden-detalle";
    public const string Intervencion = "intervencion";
    public const string Tecnicos = "tecnicos";
    public const string Formularios = "formularios";
    public const string FormBuilder = "form-builder";
    public const string Reportes = "reportes";
    public const string Usuarios = "usuarios";
    public const string UsuarioDetalle = "usuario-detalle";
    public const string Configuracion = "configuracion";
    public const string Auditoria = "auditoria";
    public const string Calibraciones = "calibraciones";
    public const string Inventario = "inventario";
}

/// <summary>Define a qué área pertenece cada ruta, para aplicar el RBAC al navegar.</summary>
public static class RutaArea
{
    private static readonly Dictionary<string, AreaAplicacion> Mapa = new()
    {
        [Rutas.Dashboard] = AreaAplicacion.Dashboard,
        [Rutas.Equipos] = AreaAplicacion.Equipos,
        [Rutas.EquipoDetalle] = AreaAplicacion.Equipos,
        [Rutas.Ordenes] = AreaAplicacion.Ordenes,
        [Rutas.OrdenDetalle] = AreaAplicacion.Ordenes,
        [Rutas.Intervencion] = AreaAplicacion.Intervencion,
        [Rutas.Tecnicos] = AreaAplicacion.Tecnicos,
        [Rutas.Formularios] = AreaAplicacion.Formularios,
        [Rutas.FormBuilder] = AreaAplicacion.Formularios,
        [Rutas.Reportes] = AreaAplicacion.Reportes,
        [Rutas.Usuarios] = AreaAplicacion.Usuarios,
        [Rutas.UsuarioDetalle] = AreaAplicacion.Usuarios,
        [Rutas.Configuracion] = AreaAplicacion.Configuracion,
        [Rutas.Auditoria] = AreaAplicacion.Auditoria,
    };

    public static AreaAplicacion? Obtener(string clave) => Mapa.TryGetValue(clave, out var a) ? a : null;
}

/// <summary>
/// Servicio de navegación entre ViewModels. Las vistas se resuelven mediante
/// DataTemplates definidos en App.xaml.
/// </summary>
public interface INavegacionService
{
    object? VistaActual { get; }
    string RutaActual { get; }
    object? ParametroActual { get; }

    event Action? Navego;

    bool PuedeNavegar(string clave);
    void Navegar(string clave, object? parametro = null);

    /// <summary>Se dispara cuando una vista solicita regresar al módulo contenedor.</summary>
    event Action<string>? RetrocederSolicitado;
    void Retroceder(string rutaDestino);
}

public class NavegacionService : INavegacionService
{
    private readonly ISesionService _sesion;
    private readonly IAuditoriaService _auditoria;
    private readonly Dictionary<string, Func<object?, object>> _fabricas = new(StringComparer.OrdinalIgnoreCase);

    public NavegacionService(ISesionService sesion, IAuditoriaService auditoria)
    {
        _sesion = sesion;
        _auditoria = auditoria;
    }

    public void Registrar(string clave, Func<object?, object> fabrica) => _fabricas[clave] = fabrica;

    public object? VistaActual { get; private set; }
    public string RutaActual { get; private set; } = string.Empty;
    public object? ParametroActual { get; private set; }
    public event Action? Navego;
    public event Action<string>? RetrocederSolicitado;

    public bool PuedeNavegar(string clave)
    {
        if (!_fabricas.ContainsKey(clave)) return false;
        var area = RutaArea.Obtener(clave);
        return area is null || _sesion.IntentarAcceso(area.Value);
    }

    public void Navegar(string clave, object? parametro = null)
    {
        if (!_fabricas.TryGetValue(clave, out var fabrica))
            return;

        var area = RutaArea.Obtener(clave);
        if (area is not null && !_sesion.IntentarAcceso(area.Value))
        {
            // Intento de acceso denegado por RBAC: queda registrado en la bitácora.
            _auditoria.Registrar(AccionAuditoria.Consulta, area.Value.ToString(), clave,
                "Intento de acceso denegado por rol");
            return;
        }

        VistaActual = fabrica(parametro);
        RutaActual = clave;
        ParametroActual = parametro;
        Navego?.Invoke();
    }

    public void Retroceder(string rutaDestino) => RetrocederSolicitado?.Invoke(rutaDestino);
}
