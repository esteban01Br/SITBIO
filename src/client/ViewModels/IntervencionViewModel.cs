using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Opción de estado de la intervención para el selector.</summary>
public record OpcionEstadoIntervencion(EstadoIntervencion Valor, string Nombre)
{
    public override string ToString() => Nombre;
};

/// <summary>Alta de repuesto consumido: contenido del diálogo modal.</summary>
public partial class FormularioRepuesto : ObservableObject
{
    [ObservableProperty] private string _codigo = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private int _cantidad = 1;
    [ObservableProperty] private decimal _precioUnitario;
}

/// <summary>
/// Ficha de intervención técnica (OBJ-05): enriquecimiento progresivo en campo
/// del diagnóstico, la causa raíz, las pruebas, los repuestos y los costos,
/// con cierre auditado sobre la orden de trabajo asociada.
/// </summary>
public partial class IntervencionViewModel : ObservableObject
{
    private readonly int _ordenId;
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    [ObservableProperty] private OrdenTrabajo? _orden;
    [ObservableProperty] private Equipo? _equipo;
    [ObservableProperty] private Intervencion? _intervencion;
    [ObservableProperty] private OpcionEstadoIntervencion? _estadoSeleccionado;
    [ObservableProperty] private string _estadoTexto = "Sin registrar";
    [ObservableProperty] private double _horas;
    [ObservableProperty] private decimal _manoObra;
    [ObservableProperty] private decimal _subtotalRepuestos;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private double _avance;
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;
    [ObservableProperty] private string _tituloDialogo = string.Empty;
    [ObservableProperty] private string _subtituloDialogo = string.Empty;
    [ObservableProperty] private string _notaTecnica = string.Empty;

    public ObservableCollection<RepuestoConsumido> Repuestos { get; } = new();
    public ObservableCollection<OpcionEstadoIntervencion> Estados { get; } = new();

    /// <summary>Nivel de escritura sobre intervenciones del rol actual.</summary>
    public bool PuedeEditar => _sesion.PuedeEditar(AreaAplicacion.Intervencion);

    /// <summary>Sin intervención registrada para la orden.</summary>
    public bool NoRegistrada => Intervencion is null;

    /// <summary>La orden solicitada no pertenece a la institución activa.</summary>
    public bool OrdenNoEncontrada => Orden is null;

    public IntervencionViewModel(int id, ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _ordenId = id;
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;

        Estados.Add(new OpcionEstadoIntervencion(EstadoIntervencion.Borrador, "Borrador"));
        Estados.Add(new OpcionEstadoIntervencion(EstadoIntervencion.EnEjecucion, "En ejecución"));
        Estados.Add(new OpcionEstadoIntervencion(EstadoIntervencion.Finalizada, "Finalizada"));
        Estados.Add(new OpcionEstadoIntervencion(EstadoIntervencion.Validada, "Validada"));

        Refrescar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    private void Refrescar()
    {
        Orden = _datos.Orden(_ordenId);
        Equipo = Orden is null ? null : _datos.Equipo(Orden.EquipoId);
        Intervencion = _datos.IntervencionDeOrden(_ordenId);

        Repuestos.Clear();

        if (Orden is null)
        {
            NotaTecnica = "La orden asociada no existe en la institución activa.";
            OnPropertyChanged(nameof(OrdenNoEncontrada));
            return;
        }

        if (Intervencion is null)
        {
            EstadoTexto = "Sin registrar";
            EstadoSeleccionado = null;
            Horas = 0;
            ManoObra = 0;
            Avance = 0;
            NotaTecnica = $"Orden {Orden.Codigo} · aún no se ha abierto la ficha de intervención técnica.";
        }
        else
        {
            EstadoSeleccionado = Estados.FirstOrDefault(e => e.Valor == Intervencion.Estado);
            EstadoTexto = Intervencion.EstadoTexto;
            Horas = Intervencion.HorasInvertidas;
            ManoObra = Intervencion.ManoObra;
            Avance = Intervencion.AvanceCampos;
            NotaTecnica = $"Orden {Orden.Codigo} · equipo {Orden.EquipoNombre} · " +
                          $"técnico {(string.IsNullOrWhiteSpace(Intervencion.TecnicoNombre) ? Orden.TecnicoNombre : Intervencion.TecnicoNombre)}";

            foreach (var repuesto in Intervencion.Repuestos)
                Repuestos.Add(repuesto);
        }

        CalcularTotal();
        OnPropertyChanged(nameof(NoRegistrada));
        OnPropertyChanged(nameof(OrdenNoEncontrada));
    }

    private void CalcularTotal()
    {
        SubtotalRepuestos = Repuestos.Sum(r => r.Subtotal);
        Total = SubtotalRepuestos + ManoObra;
    }

    partial void OnManoObraChanged(decimal value) => CalcularTotal();

    partial void OnEstadoSeleccionadoChanged(OpcionEstadoIntervencion? value)
    {
        if (value is null || Intervencion is null) return;

        Intervencion.Estado = value.Valor;
        EstadoTexto = value.Nombre;
        Avance = Intervencion.AvanceCampos;
    }

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void Volver() => _navegacion.Navegar(Rutas.OrdenDetalle, _ordenId);

    [RelayCommand]
    private void VerOrden() => _navegacion.Navegar(Rutas.OrdenDetalle, _ordenId);

    [RelayCommand]
    private void Crear()
    {
        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Intervención.", "Acceso de solo lectura");
            return;
        }

        if (Orden is null) return;

        Intervencion = new Intervencion
        {
            TenantId = TenantId,
            OrdenId = Orden.Id,
            OrdenCodigo = Orden.Codigo,
            EquipoId = Orden.EquipoId,
            EquipoNombre = Orden.EquipoNombre,
            TecnicoId = Orden.TecnicoId ?? 0,
            TecnicoNombre = string.IsNullOrWhiteSpace(Orden.TecnicoNombre)
                ? (_sesion.UsuarioActual?.NombreCompleto ?? "Sin asignar")
                : Orden.TecnicoNombre,
            Estado = EstadoIntervencion.Borrador,
            FechaInicio = DateTime.Now,
            Diagnostico = string.Empty
        };

        // El proveedor demo expone la lista subyacente; con EF Core este alta
        // se hará mediante el repositorio (fase de base de datos).
        if (_datos.Intervenciones is List<Intervencion> lista)
            lista.Add(Intervencion);

        EstadoSeleccionado = Estados[0];
        EstadoTexto = EstadoIntervencion.Borrador.ToString();
        Repuestos.Clear();
        Horas = 0;
        ManoObra = 0;
        Avance = 0;
        CalcularTotal();

        _auditoria.Registrar(AccionAuditoria.Creacion, "Intervencion", Orden.Codigo,
            "Apertura de la ficha de intervención técnica");

        _toast.Exito($"Se abrió la ficha de intervención de {Orden.Codigo}.", "Intervención creada");
        OnPropertyChanged(nameof(NoRegistrada));
    }

    [RelayCommand]
    private void Guardar()
    {
        if (Intervencion is null || Orden is null) return;

        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Intervención.", "Acceso de solo lectura");
            return;
        }

        if (string.IsNullOrWhiteSpace(Intervencion.Diagnostico))
        {
            _toast.Advertencia("Registre el diagnóstico técnico antes de guardar la intervención.", "Diagnóstico pendiente");
            return;
        }

        Intervencion.HorasInvertidas = Horas;
        Intervencion.ManoObra = ManoObra;
        Intervencion.Repuestos = Repuestos.ToList();
        if (Intervencion.Estado is EstadoIntervencion.Finalizada or EstadoIntervencion.Validada
            && Intervencion.FechaFin is null)
            Intervencion.FechaFin = DateTime.Now;

        Avance = Intervencion.AvanceCampos;

        // Sincroniza la orden cuando la intervención termina en campo.
        if (Intervencion.Estado == EstadoIntervencion.Finalizada && Orden.Estado == EstadoOrden.EnProceso)
        {
            Orden.Estado = EstadoOrden.Verificacion;
            Orden.Historial.Add(new EventoOrden
            {
                Fecha = DateTime.Now,
                Usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Usuario del sistema",
                Accion = "Intervención finalizada",
                Detalle = "Trabajo de campo completado; la orden pasa a verificación",
                Icono = "\uE73E"
            });
        }

        _auditoria.Registrar(AccionAuditoria.Actualizacion, "Intervencion", Orden.Codigo,
            $"Registro de intervención actualizado ({Formateo.Num(Horas, "0.0")} h · total {Formateo.Dinero(Total)})");

        _toast.Exito($"Se guardó la intervención de {Orden.Codigo} por {Formateo.Dinero(Total)}.", "Intervención guardada");
        Refrescar();
    }

    [RelayCommand]
    private void NuevoRepuesto()
    {
        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no puede registrar repuestos.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioRepuesto();
        TituloDialogo = "Registrar repuesto consumido";
        SubtituloDialogo = "El repuesto se descuenta del inventario del tenant";
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void ConfirmarDialogo()
    {
        if (ContenidoDialogo is FormularioRepuesto form)
        {
            if (string.IsNullOrWhiteSpace(form.Nombre))
            {
                _toast.Advertencia("Indique el nombre del repuesto.", "Datos incompletos");
                return;
            }

            if (form.Cantidad <= 0)
            {
                _toast.Advertencia("La cantidad debe ser mayor que cero.", "Cantidad inválida");
                return;
            }

            Repuestos.Add(new RepuestoConsumido
            {
                Codigo = string.IsNullOrWhiteSpace(form.Codigo) ? "S/P" : form.Codigo.Trim(),
                Nombre = form.Nombre.Trim(),
                Cantidad = form.Cantidad,
                PrecioUnitario = form.PrecioUnitario
            });

            CalcularTotal();
            _toast.Exito($"Repuesto «{form.Nombre}» agregado a la intervención.", "Repuesto registrado");
        }

        DialogoAbierto = false;
        ContenidoDialogo = null;
    }

    [RelayCommand]
    private void CancelarDialogo()
    {
        DialogoAbierto = false;
        ContenidoDialogo = null;
    }

    [RelayCommand]
    private void EliminarRepuesto(RepuestoConsumido? repuesto)
    {
        if (repuesto is null) return;

        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no puede modificar repuestos.", "Acceso de solo lectura");
            return;
        }

        Repuestos.Remove(repuesto);
        CalcularTotal();
        _toast.Informacion($"Se retiró «{repuesto.Nombre}» de la intervención.", "Repuesto eliminado");
    }
}
