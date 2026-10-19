using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Formulario de asignación de técnico: contenido del diálogo modal.</summary>
public partial class FormularioAsignacionOrden : ObservableObject
{
    [ObservableProperty] private Tecnico? _tecnicoSeleccionado;
    [ObservableProperty] private DateTime _fechaAsignacion = DateTime.Now;
}

/// <summary>
/// Detalle de la orden de trabajo (CU-04 de la DOC-01-FCP): datos del radicado
/// único, línea de tiempo inmutable de la trazabilidad, comentarios de
/// seguimiento, control de SLA y transiciones de estado auditadas.
/// </summary>
public partial class OrdenDetalleViewModel : ObservableObject
{
    private readonly int _id;
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    [ObservableProperty] private OrdenTrabajo? _orden;
    [ObservableProperty] private Equipo? _equipo;
    [ObservableProperty] private string _nuevoComentario = string.Empty;
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;
    [ObservableProperty] private string _modoDialogo = string.Empty;
    [ObservableProperty] private string _tituloDialogo = "Asignar técnico";
    [ObservableProperty] private string _subtituloDialogo = string.Empty;
    [ObservableProperty] private string _tituloPagina = "Orden de trabajo";
    [ObservableProperty] private string _subtituloPagina = string.Empty;
    [ObservableProperty] private string _nota = string.Empty;

    public ObservableCollection<EventoOrden> Historial { get; } = new();
    public ObservableCollection<ComentarioOrden> Comentarios { get; } = new();
    public ObservableCollection<Tecnico> TecnicosDisponibles { get; } = new();

    /// <summary>Nivel de escritura sobre el flujo de órdenes.</summary>
    public bool PuedeEditar => _sesion.PuedeEditar(AreaAplicacion.Ordenes);

    /// <summary>Nivel de escritura sobre intervenciones técnicas.</summary>
    public bool PuedeIntervenir => _sesion.PuedeEditar(AreaAplicacion.Intervencion);

    /// <summary>Indica que el id solicitado no corresponde a ninguna orden del tenant.</summary>
    public bool NoEncontrada => Orden is null;

    // Acciones de estado disponibles para la combinación rol + estado actual.
    public bool PuedeAsignar => PuedeEditar && Orden is { EstaAbierta: true } &&
                                Orden.Estado is EstadoOrden.Nueva or EstadoOrden.Asignada;
    public bool PuedeIniciar => PuedeEditar && Orden is { EstaAbierta: true } &&
                                Orden.Estado is EstadoOrden.Asignada or EstadoOrden.Nueva;
    public bool PuedeVerificar => PuedeEditar && Orden is { Estado: EstadoOrden.EnProceso };
    public bool PuedeCerrar => PuedeEditar && Orden is { EstaAbierta: true } &&
                               Orden.Estado is EstadoOrden.Verificacion or EstadoOrden.EnProceso;
    public bool PuedeCancelar => PuedeEditar && Orden is { EstaAbierta: true };
    public bool PuedeComentar => Orden is not null;

    public OrdenDetalleViewModel(int id, ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _id = id;
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        _exportacion = exportacion;

        Refrescar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Recarga la orden, su equipo, la línea de tiempo y los permisos.</summary>
    private void Refrescar()
    {
        var orden = Orden ?? _datos.Orden(_id);

        Orden = null;
        Orden = orden;
        Equipo = orden is null ? null : _datos.Equipo(orden.EquipoId);

        Historial.Clear();
        Comentarios.Clear();

        if (orden is not null)
        {
            TituloPagina = $"Orden {orden.Codigo}";
            SubtituloPagina = $"{orden.Titulo} · {orden.EquipoNombre}";
            Nota = orden.EstaAbierta
                ? $"Radicada el {Formateo.FechaHora(orden.FechaSolicitud)} · plazo hasta {Formateo.FechaHora(orden.FechaLimite)}"
                : $"Cerrada el {(orden.FechaCierre is null ? "—" : Formateo.FechaHora(orden.FechaCierre.Value))}";

            foreach (var evt in orden.Historial.OrderByDescending(e => e.Fecha))
                Historial.Add(evt);

            foreach (var c in orden.Comentarios.OrderByDescending(c => c.Fecha))
                Comentarios.Add(c);

            TecnicosDisponibles.Clear();
            foreach (var tecnico in _datos.TecnicosDe(TenantId))
                TecnicosDisponibles.Add(tecnico);
        }
        else
        {
            TituloPagina = "Orden no encontrada";
            SubtituloPagina = "El registro solicitado no existe en la institución activa";
            Nota = "Puede tratarse de una orden de otra institución (aislamiento multi-tenant).";
        }

        OnPropertyChanged(nameof(PuedeAsignar));
        OnPropertyChanged(nameof(NoEncontrada));
        OnPropertyChanged(nameof(PuedeIniciar));
        OnPropertyChanged(nameof(PuedeVerificar));
        OnPropertyChanged(nameof(PuedeCerrar));
        OnPropertyChanged(nameof(PuedeCancelar));
        OnPropertyChanged(nameof(PuedeComentar));
    }

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void Volver() => _navegacion.Navegar(Rutas.Ordenes);

    [RelayCommand]
    private void VerEquipo()
    {
        if (Equipo is null) return;
        _navegacion.Navegar(Rutas.EquipoDetalle, Equipo.Id);
    }

    [RelayCommand]
    private void RegistrarIntervencion()
    {
        if (Orden is null) return;

        if (!PuedeIntervenir)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Intervención.", "Acceso de solo lectura");
            return;
        }

        _navegacion.Navegar(Rutas.Intervencion, Orden.Id);
    }

    [RelayCommand]
    private void AsignarTecnico()
    {
        if (Orden is null) return;

        if (!PuedeAsignar)
        {
            _toast.Advertencia("La orden no admite asignación en su estado actual.", "Acción no disponible");
            return;
        }

        var formulario = new FormularioAsignacionOrden();
        var actual = TecnicosDisponibles.FirstOrDefault(t => t.Id == Orden.TecnicoId);
        if (actual is not null) formulario.TecnicoSeleccionado = actual;

        ModoDialogo = "asignar";
        TituloDialogo = "Asignar técnico responsable";
        SubtituloDialogo = $"{Orden.Codigo} · {Orden.Titulo}";
        ContenidoDialogo = formulario;
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Iniciar()
    {
        if (Orden is null) return;
        if (!PuedeIniciar)
        {
            _toast.Advertencia("Su rol no puede modificar el estado de la orden.", "Acceso de solo lectura");
            return;
        }

        if (Orden.TecnicoId is null)
        {
            _toast.Advertencia("Asigne un técnico antes de iniciar la ejecución.", "Técnico pendiente");
            return;
        }

        Orden.FechaInicio ??= DateTime.Now;
        CambiarEstado(EstadoOrden.EnProceso,
            $"Inicio de ejecución técnica a cargo de {Orden.TecnicoNombre}");
    }

    [RelayCommand]
    private void Verificar()
    {
        if (!PuedeVerificar)
        {
            _toast.Advertencia("La verificación solo procede con la orden en ejecución.", "Acción no disponible");
            return;
        }

        CambiarEstado(EstadoOrden.Verificacion,
            "Envío a verificación: revisión de pruebas y puesta a punto del equipo");
    }

    [RelayCommand]
    private void Cerrar()
    {
        if (!PuedeCerrar)
        {
            _toast.Advertencia("Su rol no puede cerrar órdenes en este estado.", "Acción no disponible");
            return;
        }

        CambiarEstado(EstadoOrden.Cerrada,
            "Cierre de la orden con validación del trabajo realizado");
    }

    [RelayCommand]
    private void CancelarOrden()
    {
        if (Orden is null) return;

        if (!PuedeCancelar)
        {
            _toast.Advertencia("Su rol no puede cancelar órdenes.", "Acceso de solo lectura");
            return;
        }

        ModoDialogo = "cancelar";
        TituloDialogo = "Cancelar orden de trabajo";
        SubtituloDialogo = $"{Orden.Codigo} · esta acción queda en la bitácora";
        ContenidoDialogo =
            $"La orden {Orden.Codigo} se marcará como cancelada. Esta acción queda registrada en la bitácora " +
            "inmutable con su usuario, fecha y motivo, y no puede modificarse posteriormente.";
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (Orden is null) return;

        if (ModoDialogo == "asignar")
        {
            if (ContenidoDialogo is not FormularioAsignacionOrden form || form.TecnicoSeleccionado is null)
            {
                _toast.Advertencia("Seleccione el técnico que ejecutará la orden.", "Técnico pendiente");
                return;
            }

            var tecnico = form.TecnicoSeleccionado;
            Orden.TecnicoId = tecnico.Id;
            Orden.TecnicoNombre = tecnico.Nombre;
            Orden.FechaAsignacion = DateTime.Now;
            Orden.Estado = Orden.Estado is EstadoOrden.Nueva or EstadoOrden.Cancelada
                ? EstadoOrden.Asignada
                : Orden.Estado;

            Orden.Historial.Add(new EventoOrden
            {
                Fecha = DateTime.Now,
                Usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Usuario del sistema",
                Accion = "Asignación de técnico",
                Detalle = $"Responsable asignado: {tecnico.Nombre} · {tecnico.Turno}",
                Icono = "\uE748"
            });

            _auditoria.Registrar(AccionAuditoria.Asignacion, "OrdenTrabajo", Orden.Codigo,
                $"Asignación de la orden a {tecnico.Nombre}");

            _toast.Exito($"{Orden.Codigo} quedó asignada a {tecnico.Nombre}.", "Técnico asignado");
        }
        else if (ModoDialogo == "cancelar")
        {
            Orden.Historial.Add(new EventoOrden
            {
                Fecha = DateTime.Now,
                Usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Usuario del sistema",
                Accion = "Orden cancelada",
                Detalle = "Cancelación registrada desde el detalle de la orden",
                Icono = "\uE711"
            });

            _auditoria.Registrar(AccionAuditoria.CambioEstado, "OrdenTrabajo", Orden.Codigo,
                "Cancelación de la orden de trabajo");

            _toast.Advertencia($"{Orden.Codigo} quedó cancelada y trazada en la bitácora.", "Orden cancelada");
            Orden.Estado = EstadoOrden.Cancelada;
            Orden.FechaCierre = DateTime.Now;
        }

        DialogoAbierto = false;
        ContenidoDialogo = null;
        ModoDialogo = string.Empty;
        Refrescar();
    }

    [RelayCommand]
    private void Cancelar()
    {
        DialogoAbierto = false;
        ContenidoDialogo = null;
        ModoDialogo = string.Empty;
    }

    [RelayCommand]
    private void AgregarComentario()
    {
        if (Orden is null) return;

        if (!PuedeComentar)
        {
            _toast.Advertencia("No hay orden abierta para comentar.", "Acción no disponible");
            return;
        }

        if (string.IsNullOrWhiteSpace(NuevoComentario))
        {
            _toast.Advertencia("Escriba la nota de seguimiento antes de enviarla.", "Comentario vacío");
            return;
        }

        var usuario = _sesion.UsuarioActual;

        Orden.Comentarios.Add(new ComentarioOrden
        {
            Fecha = DateTime.Now,
            Autor = usuario?.NombreCompleto ?? "Usuario del sistema",
            RolAutor = usuario?.SiglaRol ?? "---",
            Texto = NuevoComentario.Trim()
        });

        _auditoria.Registrar(AccionAuditoria.Actualizacion, "OrdenTrabajo", Orden.Codigo,
            "Nuevo comentario de seguimiento en la orden");

        NuevoComentario = string.Empty;
        _toast.Exito("La nota quedó adjunta a la orden.", "Comentario agregado");
        Refrescar();
    }

    private void CambiarEstado(EstadoOrden nuevo, string detalle)
    {
        if (Orden is null) return;

        Orden.Estado = nuevo;
        if (nuevo == EstadoOrden.EnProceso) Orden.FechaInicio ??= DateTime.Now;
        if (nuevo is EstadoOrden.Cerrada or EstadoOrden.Cancelada) Orden.FechaCierre = DateTime.Now;

        Orden.Historial.Add(new EventoOrden
        {
            Fecha = DateTime.Now,
            Usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Usuario del sistema",
            Accion = "Cambio de estado",
            Detalle = detalle,
            Icono = nuevo == EstadoOrden.Cerrada ? "\uE73E" : "\uE895"
        });

        _auditoria.Registrar(AccionAuditoria.CambioEstado, "OrdenTrabajo", Orden.Codigo,
            $"Estado actualizado a {Orden.EstadoTexto} — {detalle}");

        _toast.Exito($"{Orden.Codigo} pasó a «{Orden.EstadoTexto}».", "Estado actualizado");
        Refrescar();
    }
}
