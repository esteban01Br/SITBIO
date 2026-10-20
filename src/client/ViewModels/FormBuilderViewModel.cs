using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Nombre legible de un tipo de campo dinámico (selector del constructor).</summary>
public record OpcionTipoCampoFormulario(TipoCampoDinamico Valor, string Nombre)
{
    public override string ToString() => Nombre;
};

/// <summary>Catálogo de tipos de campo, en el orden en que se presentan al usuario.</summary>
public static class CatalogoTiposCampo
{
    public static IReadOnlyList<OpcionTipoCampoFormulario> Lista { get; } = new[]
    {
        new OpcionTipoCampoFormulario(TipoCampoDinamico.Texto, "Texto corto"),
        new OpcionTipoCampoFormulario(TipoCampoDinamico.TextoLargo, "Texto largo"),
        new OpcionTipoCampoFormulario(TipoCampoDinamico.Numero, "Número"),
        new OpcionTipoCampoFormulario(TipoCampoDinamico.Fecha, "Fecha"),
        new OpcionTipoCampoFormulario(TipoCampoDinamico.Booleano, "Sí / No"),
        new OpcionTipoCampoFormulario(TipoCampoDinamico.Seleccion, "Selección")
    };
}

/// <summary>Alta de campo en el constructor: es el contenido del diálogo modal.</summary>
public partial class CampoAlta : ObservableObject
{
    [ObservableProperty] private string _etiqueta = string.Empty;
    [ObservableProperty] private string _ayuda = string.Empty;
    [ObservableProperty] private bool _requerido = true;
    [ObservableProperty] private OpcionTipoCampoFormulario? _tipoSeleccionado;

    public IReadOnlyList<OpcionTipoCampoFormulario> Tipos => CatalogoTiposCampo.Lista;
}

/// <summary>
/// Constructor de formularios dinámicos (OBJ-06): armado de campos con tipo,
/// etiqueta, obligatoriedad y orden, vista previa en vivo y guardado auditado.
/// Con <c>id = 0</c> trabaja sobre un formulario nuevo que aún no existe en el repositorio.
/// </summary>
public partial class FormBuilderViewModel : ObservableObject
{
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    private readonly int _id;
    private readonly FormularioDinamico _formulario;

    /// <summary>Campos en edición: alimentan la lista izquierda y la vista previa.</summary>
    public ObservableCollection<CampoDinamico> Campos { get; } = new();

    /// <summary>Tipos disponibles en los selectores de tipo de campo.</summary>
    public IReadOnlyList<OpcionTipoCampoFormulario> TiposCampo => CatalogoTiposCampo.Lista;

    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>True cuando se creó un formulario nuevo (id = 0).</summary>
    public bool EsNuevo => _id == 0;

    /// <summary>Solo los roles con nivel de escritura sobre Formularios modifican la estructura.</summary>
    public bool PuedeEditarFormularios => _sesion.PuedeEditar(AreaAplicacion.Formularios);

    public string TituloFormulario => EsNuevo
        ? "Nuevo formulario"
        : string.IsNullOrWhiteSpace(Nombre) ? "Formulario sin nombre" : Nombre;

    public string SubtituloFormulario => string.IsNullOrWhiteSpace(_formulario.Categoria)
        ? "Constructor de campos dinámicos · defina la estructura en la columna izquierda"
        : $"{_formulario.Categoria} · versión {_formulario.Version} · {Campos.Count} campos";

    public string Contador => Formateo.Entero(Campos.Count);

    public string NotaContador => Campos.Count == 1 ? "campo definido" : "campos definidos";

    public FormBuilderViewModel(int id, ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;
        _id = id;

        var existente = id > 0 ? _datos.Formulario(id) : null;

        _formulario = existente ?? new FormularioDinamico
        {
            TenantId = TenantId,
            Nombre = string.Empty,
            Categoria = string.Empty,
            Descripcion = string.Empty,
            Activo = true,
            FechaCreacion = DateTime.Now,
            Autor = _sesion.UsuarioActual?.NombreCompleto ?? "Sistema",
            Version = 1,
            Aplicaciones = 0
        };

        if (existente is not null)
        {
            Nombre = existente.Nombre;
            foreach (var campo in existente.Campos.OrderBy(c => c.Orden))
                Campos.Add(campo);
        }
        else if (id > 0)
        {
            _toast.Error("No se encontró el formulario solicitado; se abrirá uno nuevo en memoria.",
                "Formulario no disponible");
        }

        // Avisos de encabezado y de los botones de ordenar cuando cambia la lista.
        Campos.CollectionChanged += (_, _) => Refrescar();
        Refrescar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Actualiza los textos derivados y el estado de subir/bajar.</summary>
    private void Refrescar()
    {
        OnPropertyChanged(nameof(Contador));
        OnPropertyChanged(nameof(NotaContador));
        OnPropertyChanged(nameof(SubtituloFormulario));
        SubirCampoCommand.NotifyCanExecuteChanged();
        BajarCampoCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Vuelve a numerar los campos en el orden actual de la lista.</summary>
    private void Reindexar()
    {
        for (var i = 0; i < Campos.Count; i++)
            Campos[i].Orden = i + 1;
    }

    partial void OnNombreChanged(string value) => OnPropertyChanged(nameof(TituloFormulario));

    // ------------------------------- Comandos -------------------------------

    [RelayCommand]
    private void AgregarCampo()
    {
        if (!PuedeEditarFormularios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Formularios.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new CampoAlta
        {
            TipoSeleccionado = CatalogoTiposCampo.Lista[0],
            Requerido = true
        };
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void AceptarCampo()
    {
        if (ContenidoDialogo is not CampoAlta alta) return;

        if (!PuedeEditarFormularios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Formularios.", "Acceso de solo lectura");
            return;
        }

        if (string.IsNullOrWhiteSpace(alta.Etiqueta))
        {
            _toast.Advertencia("Ingrese la etiqueta del campo: es el texto que verá el usuario.", "Etiqueta pendiente");
            return;
        }

        var campo = new CampoDinamico
        {
            Id = Campos.Count == 0 ? 1 : Campos.Max(c => c.Id) + 1,
            Etiqueta = alta.Etiqueta.Trim(),
            Tipo = alta.TipoSeleccionado?.Valor ?? TipoCampoDinamico.Texto,
            Requerido = alta.Requerido,
            Ayuda = string.IsNullOrWhiteSpace(alta.Ayuda) ? string.Empty : alta.Ayuda.Trim(),
            Opciones = new List<string>(),
            Orden = Campos.Count + 1
        };

        Campos.Add(campo);
        DialogoAbierto = false;
        ContenidoDialogo = null;

        _toast.Exito($"Se agregó el campo «{campo.Etiqueta}» al formulario.", "Campo agregado");
    }

    [RelayCommand]
    private void EliminarCampo(CampoDinamico? campo)
    {
        if (campo is null) return;

        if (!PuedeEditarFormularios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Formularios.", "Acceso de solo lectura");
            return;
        }

        var etiqueta = campo.Etiqueta;
        if (Campos.Remove(campo))
        {
            Reindexar();
            _toast.Informacion($"Se retiró el campo «{etiqueta}» del formulario.", "Campo eliminado");
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeSubir))]
    private void SubirCampo(CampoDinamico? campo)
    {
        if (campo is null) return;
        var indice = Campos.IndexOf(campo);
        if (indice <= 0) return;

        Campos.Move(indice, indice - 1);
        Reindexar();
    }

    [RelayCommand(CanExecute = nameof(PuedeBajar))]
    private void BajarCampo(CampoDinamico? campo)
    {
        if (campo is null) return;
        var indice = Campos.IndexOf(campo);
        if (indice < 0 || indice >= Campos.Count - 1) return;

        Campos.Move(indice, indice + 1);
        Reindexar();
    }

    private bool PuedeSubir(CampoDinamico? campo) => campo is not null && Campos.IndexOf(campo) > 0;

    private bool PuedeBajar(CampoDinamico? campo)
    {
        if (campo is null) return false;
        var indice = Campos.IndexOf(campo);
        return indice >= 0 && indice < Campos.Count - 1;
    }

    [RelayCommand]
    private void GuardarFormulario()
    {
        if (!PuedeEditarFormularios)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Formularios.", "Acceso de solo lectura");
            return;
        }

        if (string.IsNullOrWhiteSpace(Nombre))
        {
            _toast.Advertencia("Ingrese el nombre del formulario antes de guardarlo.", "Nombre pendiente");
            return;
        }

        if (Campos.Count == 0)
        {
            _toast.Advertencia("Agregue al menos un campo antes de guardar el formulario.", "Sin campos");
            return;
        }

        Reindexar();
        _formulario.Nombre = Nombre.Trim();
        _formulario.Campos = Campos.ToList();

        var obligatorios = Campos.Count(c => c.Requerido);

        if (!EsNuevo && _formulario.Id > 0)
        {
            _datos.ActualizarFormulario(_formulario);

            _auditoria.Registrar(AccionAuditoria.Actualizacion, "Formulario", _formulario.Nombre,
                $"Actualización de la estructura: {Campos.Count} campos, {obligatorios} obligatorios");

            _toast.Exito($"El formulario «{_formulario.Nombre}» quedó actualizado con {Campos.Count} campos.",
                "Formulario actualizado");
        }
        else
        {
            _formulario.TenantId = TenantId;
            _formulario.FechaCreacion = DateTime.Now;
            _formulario.Autor = _sesion.UsuarioActual?.NombreCompleto ?? "Sistema";
            _formulario.Activo = true;
            _formulario.Version = 1;
            _formulario.Aplicaciones = 0;

            _datos.AgregarFormulario(_formulario);

            _auditoria.Registrar(AccionAuditoria.Creacion, "Formulario", _formulario.Nombre,
                $"Alta desde el constructor con {Campos.Count} campos, {obligatorios} obligatorios");

            _toast.Exito($"Se creó el formulario «{_formulario.Nombre}» con {Campos.Count} campos.",
                "Formulario guardado");
        }

        Volver();
    }

    /// <summary>Regresa al módulo contenedor de formularios.</summary>
    [RelayCommand]
    private void Volver() => _navegacion.Navegar(Rutas.Formularios);

    [RelayCommand]
    private void Cancelar()
    {
        if (DialogoAbierto)
        {
            DialogoAbierto = false;
            ContenidoDialogo = null;
            return;
        }

        Volver();
    }
}
