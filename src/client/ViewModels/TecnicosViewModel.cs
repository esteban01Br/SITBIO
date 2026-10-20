using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>
/// Ficha de presentación de un técnico en la rejilla del módulo: une el modelo
/// con la carga semanal, la disponibilidad y sus órdenes activas.
/// </summary>
public class FichaTecnico
{
    public Tecnico Tecnico { get; init; } = null!;

    /// <summary>Órdenes abiertas asignadas a este técnico (trazabilidad del tenant).</summary>
    public ObservableCollection<OrdenTrabajo> OrdenesActivas { get; } = new();

    public string Iniciales => Tecnico.Iniciales;
    public string Nombre => Tecnico.Nombre;
    public string Especialidades => Tecnico.EspecialidadesTexto;
    public string Turno => $"Turno · {Tecnico.Turno}";
    public string Disponibilidad => Tecnico.DisponibilidadTexto;
    public int TotalOrdenesActivas => OrdenesActivas.Count;

    /// <summary>Carga actual 0-100 (HorasAsignadas / CapacidadHorasSemana).</summary>
    public double Carga => Math.Clamp(Tecnico.Carga, 0, 100);
    public string CargaTexto => Formateo.Porcentaje(Carga);
    public string HorasTexto =>
        $"{Formateo.Num(Tecnico.HorasAsignadas, "0.#")} de {Formateo.Num(Tecnico.CapacidadHorasSemana, "0.#")} h asignadas";

    /// <summary>Color del chip de disponibilidad (hex, se convierte con CvtHex).</summary>
    public string DisponibilidadColor => Tecnico.Disponibilidad switch
    {
        DisponibilidadTecnico.Disponible => Paleta.ExitoHex,
        DisponibilidadTecnico.EnCampo => Paleta.InfoHex,
        DisponibilidadTecnico.EnDescanso => Paleta.AdvertenciaHex,
        _ => Paleta.PeligroHex
    };

    public string DisponibilidadFondo => Tecnico.Disponibilidad switch
    {
        DisponibilidadTecnico.Disponible => Paleta.ExitoSuaveHex,
        DisponibilidadTecnico.EnCampo => Paleta.InfoSuaveHex,
        DisponibilidadTecnico.EnDescanso => Paleta.AdvertenciaSuaveHex,
        _ => Paleta.PeligroSuaveHex
    };
}

/// <summary>Formulario de asignación de orden: es el contenido del diálogo modal.</summary>
public partial class FormularioAsignacion : ObservableObject
{
    [ObservableProperty] private OrdenTrabajo? _ordenSeleccionada;
    [ObservableProperty] private FichaTecnico? _tecnicoSeleccionado;

    public ObservableCollection<OrdenTrabajo> Ordenes { get; } = new();
    public ObservableCollection<FichaTecnico> Tecnicos { get; } = new();
}

/// <summary>
/// Módulo de técnicos: plantilla del área biomédica con carga semanal,
/// disponibilidad y asignación auditada de órdenes de trabajo (CU de asignación).
/// </summary>
public partial class TecnicosViewModel : ObservableObject
{
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;

    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<FichaTecnico> Tecnicos { get; } = new();

    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "técnicos en plantilla";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>Solo los roles con nivel Total sobre Técnicos asignan órdenes.</summary>
    public bool PuedeAsignar => _sesion.PuedeEditar(AreaAplicacion.Tecnicos);

    public TecnicosViewModel(ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _sesion = sesion;
        _datos = datos;
        _auditoria = auditoria;
        _toast = toast;
        _navegacion = navegacion;
        // El servicio de exportación se recibe por el contrato del módulo (firma estándar).
        _ = exportacion;

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Reconstruye las fichas técnicas con sus órdenes activas y los KPI.</summary>
    public void Cargar()
    {
        var ordenes = _datos.OrdenesDe(TenantId).ToList();

        Tecnicos.Clear();
        foreach (var tecnico in _datos.TecnicosDe(TenantId).OrderBy(t => t.Nombre, StringComparer.CurrentCultureIgnoreCase))
        {
            var ficha = new FichaTecnico { Tecnico = tecnico };

            foreach (var orden in ordenes
                         .Where(o => o.TecnicoId == tecnico.Id && o.EstaAbierta)
                         .OrderByDescending(o => o.Prioridad == Prioridad.Critica)
                         .ThenBy(o => o.FechaSolicitud))
                ficha.OrdenesActivas.Add(orden);

            Tecnicos.Add(ficha);
        }

        var total = Tecnicos.Count;
        var conOrdenes = Tecnicos.Count(f => f.TotalOrdenesActivas > 0);
        var disponibles = Tecnicos.Count(f => f.Tecnico.Disponibilidad == DisponibilidadTecnico.Disponible);
        var asignadas = Tecnicos.Sum(f => f.TotalOrdenesActivas);
        var disponibilidadPromedio = total == 0
            ? 100d
            : Tecnicos.Average(f => 100 - f.Carga);

        // ---------------- Tarjetas de indicadores ----------------
        Kpis.Clear();

        Kpis.Add(TarjetaKpi.Crear(
            "Técnicos en plantilla",
            Formateo.Entero(total),
            "\uE716", "info",
            delta: $"{Formateo.Entero(disponibles)} disponibles ahora",
            deltaIcono: "\uE73E",
            leyenda: "personal del área biomédica",
            ayuda: "Perfiles técnicos de la institución activa con especialidades, turno y capacidad semanal registrada."));

        Kpis.Add(TarjetaKpi.Crear(
            "Con órdenes activas",
            Formateo.Entero(conOrdenes),
            "\uE90F", conOrdenes > 0 ? "acento" : "info",
            delta: $"{Formateo.Entero(asignadas)} órdenes en curso",
            deltaIcono: "\uE748",
            leyenda: "ocupados esta semana",
            ayuda: "Técnicos con al menos una orden abierta asignada; el resto está liberado para nuevas solicitudes."));

        Kpis.Add(TarjetaKpi.Crear(
            "Disponibilidad promedio",
            Formateo.Porcentaje(disponibilidadPromedio),
            "\uE823", disponibilidadPromedio >= 70 ? "exito" : disponibilidadPromedio >= 40 ? "advertencia" : "peligro",
            delta: "capacidad libre semanal",
            deltaIcono: "\uE74A",
            leyenda: "promedio de la plantilla",
            ayuda: "Capacidad no comprometida de la plantilla: 100 % menos la carga semanal promediada por técnico."));

        Contador = Formateo.Entero(total);
        NotaContador = "técnicos en plantilla";
    }

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void VerOrden(OrdenTrabajo? orden)
    {
        if (orden is null) return;
        _navegacion.Navegar(Rutas.OrdenDetalle, orden.Id);
    }

    [RelayCommand]
    private void AsignarOrden(FichaTecnico? tecnico)
    {
        if (!PuedeAsignar)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Técnicos.", "Acceso de solo lectura");
            return;
        }

        if (Tecnicos.Count == 0)
        {
            _toast.Advertencia("La institución no tiene técnicos registrados en plantilla.", "Sin técnicos");
            return;
        }

        var ordenes = _datos.OrdenesDe(TenantId)
            .Where(o => o.EstaAbierta && o.TecnicoId is null)
            .OrderByDescending(o => o.Prioridad == Prioridad.Critica)
            .ThenBy(o => o.FechaSolicitud)
            .ToList();

        if (ordenes.Count == 0)
        {
            _toast.Informacion("No hay órdenes abiertas sin técnico asignado en esta institución.", "Nada por asignar");
            return;
        }

        var form = new FormularioAsignacion();
        foreach (var orden in ordenes) form.Ordenes.Add(orden);
        foreach (var ficha in Tecnicos) form.Tecnicos.Add(ficha);

        form.OrdenSeleccionada = ordenes[0];
        form.TecnicoSeleccionado = tecnico ?? Tecnicos.FirstOrDefault(f =>
                                      f.Tecnico.Disponibilidad == DisponibilidadTecnico.Disponible)
                                  ?? Tecnicos[0];

        ContenidoDialogo = form;
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (ContenidoDialogo is not FormularioAsignacion form) return;

        if (form.OrdenSeleccionada is null)
        {
            _toast.Advertencia("Seleccione la orden de trabajo que desea asignar.", "Orden pendiente");
            return;
        }

        if (form.TecnicoSeleccionado is null)
        {
            _toast.Advertencia("Seleccione el técnico responsable de la intervención.", "Técnico pendiente");
            return;
        }

        var orden = form.OrdenSeleccionada;
        var tecnico = form.TecnicoSeleccionado.Tecnico;

        orden.TecnicoId = tecnico.Id;
        orden.TecnicoNombre = tecnico.Nombre;
        orden.Estado = EstadoOrden.Asignada;
        orden.FechaAsignacion = DateTime.Now;

        orden.Historial.Add(new EventoOrden
        {
            Fecha = DateTime.Now,
            Usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Sistema",
            Accion = "Técnico asignado",
            Detalle = $"{tecnico.Nombre} asume la orden ({tecnico.EspecialidadesTexto})",
            Icono = "\uE748"
        });

        _datos.ActualizarOrden(orden);

        _auditoria.Registrar(AccionAuditoria.Asignacion, "OrdenTrabajo", orden.Codigo,
            $"Asignación de la orden a {tecnico.Nombre} ({tecnico.EspecialidadesTexto})");

        _toast.Exito($"La orden {orden.Codigo} quedó asignada a {tecnico.Nombre}.", "Asignación registrada");

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
