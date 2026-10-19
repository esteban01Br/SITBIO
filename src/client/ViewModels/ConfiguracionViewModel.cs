using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Opción de tipo presentada en el selector de tipos de campo.</summary>
public record OpcionTipoCampo(TipoCampoDinamico Tipo, string Nombre)
{
    public override string ToString() => Nombre;
};

/// <summary>Catálogo legible de los tipos de campo dinámico disponibles.</summary>
public static class TiposCampo
{
    public static string Nombre(TipoCampoDinamico tipo) => tipo switch
    {
        TipoCampoDinamico.Texto => "Texto corto",
        TipoCampoDinamico.TextoLargo => "Texto largo",
        TipoCampoDinamico.Numero => "Número",
        TipoCampoDinamico.Fecha => "Fecha",
        TipoCampoDinamico.Booleano => "Sí / No",
        TipoCampoDinamico.Seleccion => "Selección",
        _ => tipo.ToString()
    };

    public static IReadOnlyList<OpcionTipoCampo> Lista { get; } =
        Enum.GetValues<TipoCampoDinamico>().Select(t => new OpcionTipoCampo(t, Nombre(t))).ToList();
}

/// <summary>Contenido del diálogo de alta de campo dinámico del tenant.</summary>
public partial class FormularioCampo : ObservableObject
{
    [ObservableProperty] private string _etiqueta = string.Empty;
    [ObservableProperty] private OpcionTipoCampo? _tipoSeleccionado;
    [ObservableProperty] private bool _requerido = true;

    public IReadOnlyList<OpcionTipoCampo> Tipos => TiposCampo.Lista;
}

/// <summary>Fila de la matriz completa de permisos: nivel de cada rol sobre un área.</summary>
public class FilaPermisos
{
    public AreaAplicacion Area { get; init; }
    public string AreaTexto { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public NivelAcceso Administrador { get; init; }
    public NivelAcceso Coordinador { get; init; }
    public NivelAcceso Tecnico { get; init; }
    public NivelAcceso PersonalClinico { get; init; }
    public NivelAcceso Director { get; init; }
    public NivelAcceso Regulador { get; init; }
}

/// <summary>Elemento de la leyenda de niveles de la matriz de permisos.</summary>
public class LeyendaPermiso
{
    public NivelAcceso Nivel { get; init; }
}

/// <summary>
/// Configuración del tenant: institución activa, parámetros operativos,
/// campos dinámicos por tenant (OBJ-06) y matriz completa de permisos RBAC.
/// </summary>
public partial class ConfiguracionViewModel : ObservableObject
{
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;

    public ObservableCollection<CampoDinamico> CamposDinamicos { get; } = new();
    public ObservableCollection<FilaPermisos> Permisos { get; } = new();
    public ObservableCollection<LeyendaPermiso> Leyenda { get; } = new();

    [ObservableProperty] private int _horasSlaPorDefecto = 24;
    [ObservableProperty] private int _diasPreventivos = 30;
    [ObservableProperty] private int _vigenciaCalibraciones = 365;
    [ObservableProperty] private string _prefijoCodigo = "SIT-YYYY-NNNNNN";
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "campos dinámicos del tenant";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>Los parámetros y el catálogo de campos solo se escriben con nivel Total.</summary>
    public bool PuedeEditarConfiguracion => _sesion.PuedeEditar(AreaAplicacion.Configuracion);

    /// <summary>Institución activa de la sesión (multi-tenant, RSK-03).</summary>
    public Institucion? Institucion => _sesion.TenantActual;

    public string PlanTexto => Institucion?.Plan switch
    {
        PlanSaaS.Basico => "Básico",
        PlanSaaS.Profesional => "Profesional",
        PlanSaaS.Institucional => "Institucional",
        _ => "—"
    };

    public string EstadoTexto => Institucion?.Estado switch
    {
        EstadoTenant.Activo => "Activo",
        EstadoTenant.EnPrueba => "En pruebas",
        EstadoTenant.Suspendido => "Suspendido",
        _ => "—"
    };

    public ConfiguracionViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;

        // Este módulo no navega ni exporta: los servicios se reciben para
        // respetar la firma única de módulos de lista del contrato.
        _ = navegacion;
        _ = exportacion;

        ConstruirPermisos();
        CargarCampos();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Matriz completa: filas = áreas, columnas = los 6 roles del sistema.</summary>
    private void ConstruirPermisos()
    {
        Permisos.Clear();
        foreach (var area in Enum.GetValues<AreaAplicacion>())
        {
            Permisos.Add(new FilaPermisos
            {
                Area = area,
                AreaTexto = TituloArea(area),
                Descripcion = PropositoArea(area),
                Administrador = PermisosRbac.Nivel(RolSistema.Administrador, area),
                Coordinador = PermisosRbac.Nivel(RolSistema.CoordinadorBiomedico, area),
                Tecnico = PermisosRbac.Nivel(RolSistema.Tecnico, area),
                PersonalClinico = PermisosRbac.Nivel(RolSistema.PersonalClinico, area),
                Director = PermisosRbac.Nivel(RolSistema.Director, area),
                Regulador = PermisosRbac.Nivel(RolSistema.Regulador, area)
            });
        }
    }

    /// <summary>Campos dinámicos definidos por la institución (OBJ-06).</summary>
    private void CargarCampos()
    {
        foreach (var campo in _datos.FormulariosDe(TenantId).SelectMany(f => f.Campos))
            CamposDinamicos.Add(campo);

        Contador = Formateo.Entero(CamposDinamicos.Count);
        NotaContador = "campos dinámicos del tenant";

        Leyenda.Clear();
        Leyenda.Add(new LeyendaPermiso { Nivel = NivelAcceso.Total });
        Leyenda.Add(new LeyendaPermiso { Nivel = NivelAcceso.Lectura });
        Leyenda.Add(new LeyendaPermiso { Nivel = NivelAcceso.Ninguno });
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
        AreaAplicacion.Dashboard => "Indicadores y alertas",
        AreaAplicacion.Equipos => "Parque biomédico",
        AreaAplicacion.Ordenes => "Solicitudes y SLA",
        AreaAplicacion.Intervencion => "Ejecución y firmas",
        AreaAplicacion.Tecnicos => "Cuadrillas y carga",
        AreaAplicacion.Formularios => "Plantillas del tenant",
        AreaAplicacion.Reportes => "Informes exportables",
        AreaAplicacion.Usuarios => "Padrón de cuentas",
        AreaAplicacion.Configuracion => "Parámetros del tenant",
        AreaAplicacion.Auditoria => "Bitácora inmutable",
        _ => "Área del sistema"
    };

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void GuardarParametros()
    {
        if (!PuedeEditarConfiguracion)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Configuración.", "Acceso de solo lectura");
            return;
        }

        if (HorasSlaPorDefecto < 1 || HorasSlaPorDefecto > 720)
        {
            _toast.Advertencia("Las horas de SLA por defecto deben estar entre 1 y 720.", "Parámetro inválido");
            return;
        }

        if (DiasPreventivos < 1 || DiasPreventivos > 365)
        {
            _toast.Advertencia("Los días de anticipación de preventivos deben estar entre 1 y 365.", "Parámetro inválido");
            return;
        }

        if (VigenciaCalibraciones < 30 || VigenciaCalibraciones > 730)
        {
            _toast.Advertencia("La vigencia de calibraciones debe estar entre 30 y 730 días.", "Parámetro inválido");
            return;
        }

        if (string.IsNullOrWhiteSpace(PrefijoCodigo))
        {
            _toast.Advertencia("Ingrese el prefijo de código de las órdenes (formato SIT-YYYY-NNNNNN).", "Prefijo vacío");
            return;
        }

        _auditoria.Registrar(AccionAuditoria.Actualizacion, "Configuracion", Institucion?.Nombre ?? "Institución",
            $"Parámetros operativos: SLA {HorasSlaPorDefecto} h, preventivos cada {DiasPreventivos} días, " +
            $"vigencia de calibraciones {VigenciaCalibraciones} días, prefijo de código {PrefijoCodigo}");

        _toast.Exito("Los parámetros operativos de la institución quedaron actualizados.", "Parámetros guardados");
    }

    [RelayCommand]
    private void AgregarCampo()
    {
        if (!PuedeEditarConfiguracion)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Configuración.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioCampo
        {
            TipoSeleccionado = TiposCampo.Lista.FirstOrDefault()
        };
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void GuardarCampo()
    {
        if (ContenidoDialogo is not FormularioCampo form) return;

        if (string.IsNullOrWhiteSpace(form.Etiqueta))
        {
            _toast.Advertencia("Ingrese la etiqueta visible del campo dinámico.", "Datos incompletos");
            return;
        }

        if (form.TipoSeleccionado is null)
        {
            _toast.Advertencia("Seleccione el tipo de dato del campo.", "Tipo pendiente");
            return;
        }

        if (CamposDinamicos.Any(c => c.Etiqueta.Equals(form.Etiqueta.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            _toast.Advertencia("Ya existe un campo con esa etiqueta en esta institución.", "Campo duplicado");
            return;
        }

        var campo = new CampoDinamico
        {
            Etiqueta = form.Etiqueta.Trim(),
            Tipo = form.TipoSeleccionado.Tipo,
            Requerido = form.Requerido,
            Orden = CamposDinamicos.Count + 1
        };

        // Se conserva en memoria ligado a los formularios del tenant (OBJ-06).
        _datos.FormulariosDe(TenantId).FirstOrDefault()?.Campos.Add(campo);
        CamposDinamicos.Add(campo);
        Contador = Formateo.Entero(CamposDinamicos.Count);

        _auditoria.Registrar(AccionAuditoria.Creacion, "CampoDinamico", campo.Etiqueta,
            $"Alta del campo «{campo.Etiqueta}» ({campo.TipoTexto}) {(campo.Requerido ? "obligatorio" : "opcional")}");

        _toast.Exito($"El campo «{campo.Etiqueta}» ya está disponible en los formularios del tenant.", "Campo agregado");

        DialogoAbierto = false;
        ContenidoDialogo = null;
    }

    [RelayCommand]
    private void CancelarCampo()
    {
        DialogoAbierto = false;
        ContenidoDialogo = null;
    }

    [RelayCommand]
    private void EliminarCampo(CampoDinamico? campo)
    {
        if (campo is null) return;

        if (!PuedeEditarConfiguracion)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Configuración.", "Acceso de solo lectura");
            return;
        }

        foreach (var formulario in _datos.FormulariosDe(TenantId))
        {
            if (formulario.Campos.Remove(campo)) break;
        }

        CamposDinamicos.Remove(campo);
        Contador = Formateo.Entero(CamposDinamicos.Count);

        _auditoria.Registrar(AccionAuditoria.Eliminacion, "CampoDinamico", campo.Etiqueta,
            $"Baja del campo «{campo.Etiqueta}» ({campo.TipoTexto}) en los formularios del tenant");

        _toast.Exito($"El campo «{campo.Etiqueta}» se eliminó de la configuración del tenant.", "Campo eliminado");
    }
}
