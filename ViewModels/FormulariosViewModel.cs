using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Formulario de alta de formulario dinámico: es el contenido del diálogo modal.</summary>
public partial class FormularioDinamicoAlta : ObservableObject
{
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _categoria = string.Empty;
    [ObservableProperty] private string _descripcion = string.Empty;
}

/// <summary>
/// Módulo de formularios dinámicos (OBJ-06): catálogo de formularios de
/// inspección y registro por institución, con alta, baja y edición auditadas
/// que abren el constructor de campos (FormBuilder).
/// </summary>
public partial class FormulariosViewModel : ObservableObject
{
    private const string TodosLosEstados = "Todos los estados";

    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    public ObservableCollection<FormularioDinamico> Formularios { get; } = new();
    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<string> EstadosFiltro { get; } = new();

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private string _estadoFiltro = TodosLosEstados;
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "formularios en la institución";

    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;
    [ObservableProperty] private string _dialogoTitulo = "Nuevo formulario";
    [ObservableProperty] private string _dialogoSubtitulo = string.Empty;
    [ObservableProperty] private string _dialogoIcono = "\uE710";
    [ObservableProperty] private string _dialogoTextoConfirmar = "Guardar";
    [ObservableProperty] private string _dialogoTextoCancelar = "Cancelar";
    [ObservableProperty] private bool _dialogoEsPeligro;

    /// <summary>Formulario en espera de confirmación para la baja definitiva.</summary>
    private FormularioDinamico? _pendienteEliminacion;

    /// <summary>Solo los roles con nivel de escritura sobre Formularios dan de alta o de baja.</summary>
    public bool PuedeEditarFormularios => _sesion.PuedeEditar(AreaAplicacion.Formularios);

    public FormulariosViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;

        EstadosFiltro.Add(TodosLosEstados);
        EstadosFiltro.Add("Activos");
        EstadosFiltro.Add("Inactivos");

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Reconstruye los KPI del módulo y vuelve a aplicar los filtros activos.</summary>
    public void Cargar()
    {
        var formularios = _datos.FormulariosDe(TenantId).ToList();

        Kpis.Clear();
        var total = formularios.Count;
        var activos = formularios.Count(f => f.Activo);
        var campos = formularios.Sum(f => f.TotalCampos);
        var requeridos = formularios.Sum(f => f.CamposRequeridos);
        var aplicaciones = formularios.Sum(f => f.Aplicaciones);

        Kpis.Add(TarjetaKpi.Crear(
            "Formularios registrados",
            Formateo.Entero(total),
            "\uE729", "info",
            delta: $"{Formateo.Entero(activos)} activos de {Formateo.Entero(total)}",
            deltaIcono: "\uE73E",
            leyenda: "estructuras dinámicas",
            ayuda: "Formularios de inspección y registro disponibles para la institución (aislamiento por tenant)."));

        Kpis.Add(TarjetaKpi.Crear(
            "Formularios activos",
            Formateo.Entero(activos),
            "\uE73E",
            activos == total ? "exito" : "advertencia",
            delta: activos == total ? "todos habilitados" : $"{Formateo.Entero(total - activos)} inactivos",
            deltaIcono: activos == total ? "\uE73E" : "\uE7BA",
            leyenda: "en uso operativo",
            ayuda: "Formularios habilitados para aplicarse en registros; los inactivos quedan sólo para consulta."));

        Kpis.Add(TarjetaKpi.Crear(
            "Campos configurados",
            Formateo.Entero(campos),
            "\uE8A7", "info",
            delta: $"{Formateo.Entero(requeridos)} obligatorios",
            deltaIcono: "\uE748",
            leyenda: "en todos los formularios",
            ayuda: "Suma de los campos de todos los formularios; el orden se define en el constructor (FormBuilder)."));

        Kpis.Add(TarjetaKpi.Crear(
            "Aplicaciones registradas",
            Formateo.Entero(aplicaciones),
            "\uE9F9", "acento",
            delta: total == 0 ? "sin uso" : $"{Formateo.Num(aplicaciones / (double)total, "0.0")} por formulario",
            deltaIcono: "\uE74A",
            leyenda: "usos acumulados",
            ayuda: "Veces que los formularios fueron aplicados en inspecciones, recepciones y hoja de vida."));

        Contador = Formateo.Entero(total);
        NotaContador = "formularios en la institución";

        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var formularios = _datos.FormulariosDe(TenantId).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim();
            formularios = formularios.Where(f =>
                f.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                f.Categoria.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                f.Descripcion.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (EstadoFiltro == "Activos") formularios = formularios.Where(f => f.Activo);
        else if (EstadoFiltro == "Inactivos") formularios = formularios.Where(f => !f.Activo);

        Formularios.Clear();
        foreach (var formulario in formularios
                     .OrderBy(f => f.Categoria, StringComparer.CurrentCultureIgnoreCase)
                     .ThenBy(f => f.Nombre, StringComparer.CurrentCultureIgnoreCase))
            Formularios.Add(formulario);
    }

    partial void OnBusquedaChanged(string value) => AplicarFiltros();
    partial void OnEstadoFiltroChanged(string value) => AplicarFiltros();

    private void CerrarDialogo()
    {
        DialogoAbierto = false;
        ContenidoDialogo = null;
        _pendienteEliminacion = null;
    }

    // ------------------------------- Comandos -------------------------------

    [RelayCommand]
    private void LimpiarFiltros()
    {
        Busqueda = string.Empty;
        EstadoFiltro = TodosLosEstados;
        _toast.Informacion("Se restablecieron los filtros del catálogo de formularios.", "Filtros limpiados");
    }

    [RelayCommand]
    private void Nuevo()
    {
        if (!PuedeEditarFormularios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Formularios.", "Acceso de solo lectura");
            return;
        }

        _pendienteEliminacion = null;
        ContenidoDialogo = new FormularioDinamicoAlta();
        DialogoTitulo = "Nuevo formulario dinámico";
        DialogoSubtitulo = "Defina la identificación básica; después abra el constructor para estructurar sus campos.";
        DialogoIcono = "\uE710";
        DialogoTextoConfirmar = "Crear formulario";
        DialogoTextoCancelar = "Cancelar";
        DialogoEsPeligro = false;
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Eliminar(FormularioDinamico? formulario)
    {
        if (formulario is null) return;

        if (!PuedeEditarFormularios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Formularios.", "Acceso de solo lectura");
            return;
        }

        _pendienteEliminacion = formulario;
        ContenidoDialogo = null;
        DialogoTitulo = "¿Eliminar el formulario?";
        DialogoSubtitulo = $"Se retirará «{formulario.Nombre}» y sus {formulario.TotalCampos} campos de la institución. La baja queda registrada en la bitácora de auditoría.";
        DialogoIcono = "\uE74D";
        DialogoTextoConfirmar = "Eliminar";
        DialogoTextoCancelar = "Cancelar";
        DialogoEsPeligro = true;
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        // ---- Alta de formulario ----
        if (ContenidoDialogo is FormularioDinamicoAlta alta)
        {
            if (string.IsNullOrWhiteSpace(alta.Nombre))
            {
                _toast.Advertencia("Ingrese el nombre del formulario.", "Datos incompletos");
                return;
            }

            if (string.IsNullOrWhiteSpace(alta.Categoria))
            {
                _toast.Advertencia("Ingrese la categoría (ej. Seguridad, Mantenimiento, Activos).", "Categoría pendiente");
                return;
            }

            if (_datos.FormulariosDe(TenantId).Any(f =>
                    f.Nombre.Equals(alta.Nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                _toast.Error("Ya existe un formulario con ese nombre en esta institución.", "Nombre duplicado");
                return;
            }

            var nuevo = new FormularioDinamico
            {
                TenantId = TenantId,
                Nombre = alta.Nombre.Trim(),
                Categoria = alta.Categoria.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(alta.Descripcion)
                    ? "Sin descripción registrada."
                    : alta.Descripcion.Trim(),
                Activo = true,
                FechaCreacion = DateTime.Now,
                Autor = _sesion.UsuarioActual?.NombreCompleto ?? "Sistema",
                Version = 1,
                Aplicaciones = 0,
                Campos = new List<CampoDinamico>()
            };

            _datos.AgregarFormulario(nuevo);

            _auditoria.Registrar(AccionAuditoria.Creacion, "Formulario", nuevo.Nombre,
                $"Alta del formulario «{nuevo.Nombre}» en la categoría {nuevo.Categoria}");

            _toast.Exito($"Se creó el formulario «{nuevo.Nombre}»; ábralo para estructurar sus campos.",
                "Formulario creado");

            CerrarDialogo();
            Cargar();
            return;
        }

        // ---- Baja de formulario (confirmación con estilo de peligro) ----
        if (_pendienteEliminacion is { } eliminado)
        {
            _datos.EliminarFormulario(eliminado.Id);

            _auditoria.Registrar(AccionAuditoria.Eliminacion, "Formulario", eliminado.Nombre,
                $"Baja del formulario «{eliminado.Nombre}» con {eliminado.TotalCampos} campos");

            _toast.Exito($"El formulario «{eliminado.Nombre}» fue eliminado de la institución.",
                "Formulario eliminado");

            CerrarDialogo();
            Cargar();
            return;
        }

        CerrarDialogo();
    }

    [RelayCommand]
    private void Cancelar() => CerrarDialogo();

    [RelayCommand]
    private void Ver(FormularioDinamico? formulario)
    {
        if (formulario is null) return;
        _navegacion.Navegar(Rutas.FormBuilder, formulario.Id);
    }

    [RelayCommand]
    private void Editar(FormularioDinamico? formulario)
    {
        if (formulario is null) return;
        _navegacion.Navegar(Rutas.FormBuilder, formulario.Id);
    }
}
