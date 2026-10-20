using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Opción de tipo de mantenimiento para los selectores del formulario.</summary>
public record OpcionTipo(TipoMantenimiento Valor, string Nombre)
{
    public override string ToString() => Nombre;
};

/// <summary>Opción de prioridad para los selectores del formulario.</summary>
public record OpcionPrioridad(Prioridad Valor, string Nombre)
{
    public override string ToString() => Nombre;
};

/// <summary>
/// Columna del tablero Kanban: agrupa las órdenes según <see cref="OrdenTrabajo.Columna"/>
/// (metodología de tablero adaptada al dominio biomédico).
/// </summary>
public class ColumnaKanbanVista
{
    public ColumnaKanban Columna { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string Icono { get; init; } = "";
    public string Color { get; init; } = "#0B6FA4";
    public string Contador { get; set; } = "0";
    public ObservableCollection<OrdenTrabajo> Ordenes { get; } = new();
}

/// <summary>Alta de orden de trabajo: contenido del diálogo modal.</summary>
public partial class FormularioOrden : ObservableObject
{
    [ObservableProperty] private string _titulo = string.Empty;
    [ObservableProperty] private string _descripcion = string.Empty;
    [ObservableProperty] private Equipo? _equipoSeleccionado;
    [ObservableProperty] private OpcionTipo? _tipoSeleccionado;
    [ObservableProperty] private OpcionPrioridad? _prioridadSeleccionada;
    [ObservableProperty] private int _slaHoras = 24;

    public IReadOnlyList<Equipo> Equipos { get; set; } = Array.Empty<Equipo>();
    public IReadOnlyList<OpcionTipo> Tipos { get; } = new List<OpcionTipo>
    {
        new(TipoMantenimiento.Correctivo, "Correctivo"),
        new(TipoMantenimiento.Preventivo, "Preventivo"),
        new(TipoMantenimiento.Predictivo, "Predictivo"),
        new(TipoMantenimiento.Calibracion, "Calibración"),
        new(TipoMantenimiento.Instalacion, "Instalación")
    };

    public IReadOnlyList<OpcionPrioridad> Prioridades { get; } = new List<OpcionPrioridad>
    {
        new(Prioridad.Baja, "Baja"),
        new(Prioridad.Media, "Media"),
        new(Prioridad.Alta, "Alta"),
        new(Prioridad.Critica, "Crítica")
    };

    public IReadOnlyList<int> Plazos { get; } = new List<int> { 4, 8, 12, 24, 48, 72 };
}

/// <summary>
/// Módulo de órdenes de trabajo (CU-02 / CU-04 de la DOC-01-FCP): bandeja con
/// filtros y tablero Kanban conmutables, alta con código único de seguimiento
/// SIT-2026-000123 y trazabilidad completa en la bitácora.
/// </summary>
public partial class OrdenesViewModel : ObservableObject
{
    private const string TodosLosEstados = "Todos los estados";
    private const string TodasLasPrioridades = "Todas las prioridades";
    private const string TodosLosTipos = "Todos los tipos";

    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    private IReadOnlyList<OrdenTrabajo> _todas = Array.Empty<OrdenTrabajo>();

    public ObservableCollection<OrdenTrabajo> Ordenes { get; } = new();
    public ObservableCollection<ColumnaKanbanVista> Columnas { get; } = new();
    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<string> EstadosFiltro { get; } = new();
    public ObservableCollection<string> PrioridadesFiltro { get; } = new();
    public ObservableCollection<string> TiposFiltro { get; } = new();

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private string _estadoFiltro = TodosLosEstados;
    [ObservableProperty] private string _prioridadFiltro = TodasLasPrioridades;
    [ObservableProperty] private string _tipoFiltro = TodosLosTipos;
    [ObservableProperty] private bool _vistaEsKanban;
    [ObservableProperty] private string _textoVista = "Tablero Kanban";
    [ObservableProperty] private string _iconoVista = "";
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "órdenes de trabajo";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>Solo roles con nivel Total sobre Órdenes crean y avanzan órdenes.</summary>
    public bool PuedeCrearOrden => _sesion.PuedeEditar(AreaAplicacion.Ordenes);

    /// <summary>La exportación exige nivel sobre Reportes u Órdenes.</summary>
    public bool PuedeExportar =>
        _sesion.IntentarAcceso(AreaAplicacion.Reportes) ||
        _sesion.PuedeEditar(AreaAplicacion.Ordenes);

    public OrdenesViewModel(ISesionService sesion, IRepositorioDatos datos,
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
        foreach (var nombre in Enum.GetValues<EstadoOrden>().Select(EstadoTextoDe))
            EstadosFiltro.Add(nombre);

        PrioridadesFiltro.Add(TodasLasPrioridades);
        foreach (var nombre in Enum.GetValues<Prioridad>().Select(PrioridadTextoDe))
            PrioridadesFiltro.Add(nombre);

        TiposFiltro.Add(TodosLosTipos);
        foreach (var nombre in Enum.GetValues<TipoMantenimiento>().Select(TipoTextoDe))
            TiposFiltro.Add(nombre);

        ConstruirColumnas();
        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    private static string EstadoTextoDe(EstadoOrden e) => e switch
    {
        EstadoOrden.Nueva => "Nueva",
        EstadoOrden.Asignada => "Asignada",
        EstadoOrden.EnProceso => "En proceso",
        EstadoOrden.EnEsperaRepuesto => "Espera de repuesto",
        EstadoOrden.Verificacion => "Verificación",
        EstadoOrden.Cerrada => "Cerrada",
        EstadoOrden.Cancelada => "Cancelada",
        _ => e.ToString()
    };

    private static string PrioridadTextoDe(Prioridad p) => p switch
    {
        Prioridad.Critica => "Crítica",
        Prioridad.Alta => "Alta",
        Prioridad.Media => "Media",
        Prioridad.Baja => "Baja",
        _ => p.ToString()
    };

    private static string TipoTextoDe(TipoMantenimiento t) => t switch
    {
        TipoMantenimiento.Preventivo => "Preventivo",
        TipoMantenimiento.Correctivo => "Correctivo",
        TipoMantenimiento.Predictivo => "Predictivo",
        TipoMantenimiento.Calibracion => "Calibración",
        TipoMantenimiento.Instalacion => "Instalación",
        _ => t.ToString()
    };

    private void ConstruirColumnas()
    {
        Columnas.Clear();

        void Agregar(ColumnaKanban c, string titulo, string descripcion, string icono, string color)
            => Columnas.Add(new ColumnaKanbanVista
            {
                Columna = c,
                Titulo = titulo,
                Descripcion = descripcion,
                Icono = icono,
                Color = color
            });

        Agregar(ColumnaKanban.PorHacer, "Por hacer", "Nuevas y asignadas", "", "#0B6FA4");
        Agregar(ColumnaKanban.EnCurso, "En curso", "En ejecución técnica", "", "#1D7DB8");
        Agregar(ColumnaKanban.EnRevision, "En revisión", "Espera de repuesto", "", "#C98A0E");
        Agregar(ColumnaKanban.EnPruebas, "En pruebas", "Verificación y puesta a punto", "", "#6B4FD8");
        Agregar(ColumnaKanban.Completada, "Completada", "Cerradas y canceladas", "", "#1E9E62");
    }

    /// <summary>Recarga indicadores y reaplica los filtros vigentes.</summary>
    public void Cargar()
    {
        _todas = _datos.OrdenesDe(TenantId);

        var abiertas = _todas.Where(o => o.EstaAbierta).ToList();
        var criticas = abiertas.Count(o => o.Prioridad is Prioridad.Critica or Prioridad.Alta);
        var vencidas = abiertas.Count(o => o.SLAVencido);
        var sinTecnico = abiertas.Count(o => o.TecnicoId is null);
        var cerradas = _todas.Count(o => o.Estado is EstadoOrden.Cerrada);

        Kpis.Clear();
        Kpis.Add(TarjetaKpi.Crear(
            "Órdenes abiertas",
            Formateo.Entero(abiertas.Count),
            "", "info",
            delta: criticas > 0 ? $"{Formateo.Entero(criticas)} críticas o altas" : "sin criticidad alta",
            deltaIcono: criticas > 0 ? "" : "",
            leyenda: "flujo activo",
            ayuda: "Nuevas, asignadas, en proceso, espera de repuesto y verificación."));

        Kpis.Add(TarjetaKpi.Crear(
            "Fuera de plazo SLA",
            Formateo.Entero(vencidas),
            "", vencidas > 0 ? "peligro" : "exito",
            delta: vencidas > 0 ? "requiere escalar" : "todo dentro de plazo",
            deltaIcono: vencidas > 0 ? "" : "",
            leyenda: "incumplimiento",
            ayuda: "Órdenes abiertas cuyo tiempo de respuesta comprometido ya expiró."));

        Kpis.Add(TarjetaKpi.Crear(
            "Sin técnico asignado",
            Formateo.Entero(sinTecnico),
            "", sinTecnico > 0 ? "advertencia" : "exito",
            delta: sinTecnico > 0 ? "pendiente de reparto" : "todas asignadas",
            deltaIcono: sinTecnico > 0 ? "" : "",
            leyenda: "cola de asignación",
            ayuda: "Órdenes abiertas aún sin responsable técnico (paso previo a la ejecución)."));

        Kpis.Add(TarjetaKpi.Crear(
            "Órdenes cerradas",
            Formateo.Entero(cerradas),
            "", "exito",
            delta: $"{Formateo.Entero(_todas.Count)} en total",
            deltaIcono: "",
            leyenda: "histórico del tenant",
            ayuda: "Órdenes finalizadas y verificadas. Cada cierre queda firmado en la bitácora."));

        Contador = Formateo.Entero(_todas.Count);
        NotaContador = "órdenes de trabajo";

        AplicarFiltros();
    }

    private IEnumerable<OrdenTrabajo> Filtradas()
    {
        var consulta = _todas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim();
            consulta = consulta.Where(o =>
                o.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                o.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                o.EquipoNombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                o.EquipoUbicacion.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                o.TecnicoNombre.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (EstadoFiltro != TodosLosEstados)
            consulta = consulta.Where(o => EstadoTextoDe(o.Estado) == EstadoFiltro);

        if (PrioridadFiltro != TodasLasPrioridades)
            consulta = consulta.Where(o => PrioridadTextoDe(o.Prioridad) == PrioridadFiltro);

        if (TipoFiltro != TodosLosTipos)
            consulta = consulta.Where(o => TipoTextoDe(o.Tipo) == TipoFiltro);

        return consulta
            .OrderByDescending(o => o.EstaAbierta)
            .ThenByDescending(o => o.Prioridad)
            .ThenByDescending(o => o.FechaSolicitud);
    }

    private void AplicarFiltros()
    {
        var filtradas = Filtradas().ToList();

        Ordenes.Clear();
        foreach (var orden in filtradas) Ordenes.Add(orden);

        foreach (var columna in Columnas)
        {
            columna.Ordenes.Clear();
            var deLaColumna = filtradas.Where(o => o.Columna == columna.Columna).ToList();
            foreach (var orden in deLaColumna) columna.Ordenes.Add(orden);
            columna.Contador = Formateo.Entero(deLaColumna.Count);
        }

        // Refresco de encabezados de columna (objeto plano: se notifica por el ItemsSource).
        var refresco = Columnas.ToList();
        Columnas.Clear();
        foreach (var columna in refresco) Columnas.Add(columna);
    }

    partial void OnBusquedaChanged(string value) => AplicarFiltros();
    partial void OnEstadoFiltroChanged(string value) => AplicarFiltros();
    partial void OnPrioridadFiltroChanged(string value) => AplicarFiltros();
    partial void OnTipoFiltroChanged(string value) => AplicarFiltros();

    partial void OnVistaEsKanbanChanged(bool value)
    {
        TextoVista = value ? "Vista de tabla" : "Tablero Kanban";
        IconoVista = value ? "" : "";
    }

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void LimpiarFiltros()
    {
        Busqueda = string.Empty;
        EstadoFiltro = TodosLosEstados;
        PrioridadFiltro = TodasLasPrioridades;
        TipoFiltro = TodosLosTipos;
        _toast.Informacion("Se restablecieron los filtros de la bandeja de órdenes.", "Filtros limpiados");
    }

    [RelayCommand]
    private void AlternarVista() => VistaEsKanban = !VistaEsKanban;

    [RelayCommand]
    private void Actualizar()
    {
        Cargar();
        _toast.Informacion("Bandeja y tablero recargados con la información más reciente.", "Órdenes actualizadas");
    }

    [RelayCommand]
    private void Ver(OrdenTrabajo? orden)
    {
        if (orden is null) return;
        _navegacion.Navegar(Rutas.OrdenDetalle, orden.Id);
    }

    [RelayCommand]
    private void Nuevo()
    {
        if (!PuedeCrearOrden)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Órdenes de trabajo.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioOrden
        {
            Equipos = _datos.EquiposDe(TenantId),
            TipoSeleccionado = null,
            PrioridadSeleccionada = null
        };
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (ContenidoDialogo is not FormularioOrden form) return;

        if (string.IsNullOrWhiteSpace(form.Titulo))
        {
            _toast.Advertencia("Describa brevemente la falla o el trabajo solicitado.", "Título pendiente");
            return;
        }

        if (form.EquipoSeleccionado is null)
        {
            _toast.Advertencia("Seleccione el equipo biomédico sobre el cual se ejecutará la orden.", "Equipo pendiente");
            return;
        }

        if (form.TipoSeleccionado is null || form.PrioridadSeleccionada is null)
        {
            _toast.Advertencia("Defina el tipo de mantenimiento y la prioridad de respuesta.", "Clasificación pendiente");
            return;
        }

        var usuario = _sesion.UsuarioActual;
        var equipo = form.EquipoSeleccionado;
        var tipo = form.TipoSeleccionado.Valor;
        var prioridad = form.PrioridadSeleccionada.Valor;

        var orden = new OrdenTrabajo
        {
            TenantId = TenantId,
            Codigo = _datos.SiguienteCodigoOrden(TenantId),
            Titulo = form.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(form.Descripcion)
                ? "Solicitud registrada desde la bandeja de órdenes."
                : form.Descripcion.Trim(),
            EquipoId = equipo.Id,
            EquipoNombre = equipo.Nombre,
            EquipoUbicacion = equipo.Ubicacion,
            Tipo = tipo,
            Prioridad = prioridad,
            Estado = EstadoOrden.Nueva,
            SolicitanteId = usuario?.Id ?? 0,
            SolicitanteNombre = usuario?.NombreCompleto ?? "Usuario del sistema",
            FechaSolicitud = DateTime.Now,
            SLAHoras = form.SlaHoras
        };

        orden.Historial.Add(new EventoOrden
        {
            Fecha = DateTime.Now,
            Usuario = orden.SolicitanteNombre,
            Accion = "Orden creada",
            Detalle = $"Código {orden.Codigo} · mantenimiento {TipoTextoDe(tipo)} · prioridad {PrioridadTextoDe(prioridad)}",
            Icono = ""
        });

        _datos.AgregarOrden(orden);
        equipo.OrdenesAbiertas++;

        _datos.AgregarNotificacion(new Notificacion
        {
            TenantId = TenantId,
            Titulo = $"Nueva orden {orden.Codigo}",
            Mensaje = $"{orden.Titulo} · {equipo.Nombre} ({equipo.Ubicacion})",
            Fecha = DateTime.Now,
            Leida = false,
            Tipo = prioridad is Prioridad.Critica or Prioridad.Alta ? "danger" : "info",
            Icono = ""
        });

        _auditoria.Registrar(AccionAuditoria.Creacion, "OrdenTrabajo", orden.Codigo,
            $"Alta de orden {TipoTextoDe(tipo)} para {equipo.Nombre} · prioridad {PrioridadTextoDe(prioridad)}");

        _toast.Exito($"Se creó {orden.Codigo} para {equipo.Nombre}.", "Orden de trabajo creada");

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
    private void Avanzar(OrdenTrabajo? orden)
    {
        if (orden is null) return;

        if (!PuedeCrearOrden)
        {
            _toast.Advertencia("Su rol no puede modificar el estado de las órdenes.", "Acceso de solo lectura");
            return;
        }

        if (!orden.EstaAbierta)
        {
            _toast.Informacion($"{orden.Codigo} ya se encuentra finalizada.", "Orden finalizada");
            return;
        }

        var siguiente = orden.Estado switch
        {
            EstadoOrden.Nueva => EstadoOrden.Asignada,
            EstadoOrden.Asignada => EstadoOrden.EnProceso,
            EstadoOrden.EnProceso => EstadoOrden.Verificacion,
            EstadoOrden.EnEsperaRepuesto => EstadoOrden.EnProceso,
            EstadoOrden.Verificacion => EstadoOrden.Cerrada,
            _ => orden.Estado
        };

        if (siguiente == orden.Estado) return;

        var usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Usuario del sistema";
        orden.Estado = siguiente;
        if (siguiente == EstadoOrden.Cerrada) orden.FechaCierre = DateTime.Now;

        orden.Historial.Add(new EventoOrden
        {
            Fecha = DateTime.Now,
            Usuario = usuario,
            Accion = "Cambio de estado",
            Detalle = $"Estado actualizado a {EstadoTextoDe(siguiente)} desde la bandeja de órdenes",
            Icono = siguiente == EstadoOrden.Cerrada ? "" : ""
        });

        _auditoria.Registrar(AccionAuditoria.CambioEstado, "OrdenTrabajo", orden.Codigo,
            $"Paso de estado a {EstadoTextoDe(siguiente)}");

        _datos.ActualizarOrden(orden);

        _toast.Exito($"{orden.Codigo} pasó a «{EstadoTextoDe(siguiente)}».", "Estado actualizado");
        Cargar();
    }

    [RelayCommand]
    private void ExportarExcel()
    {
        if (!PuedeExportar)
        {
            _toast.Advertencia("Su rol no tiene permisos de exportación.", "Acceso de solo lectura");
            return;
        }

        if (Ordenes.Count == 0)
        {
            _toast.Advertencia("No hay órdenes que coincidan con los filtros activos.", "Nada para exportar");
            return;
        }

        _exportacion.ExportarOrdenesExcel(TenantId, Ordenes.ToList());
    }

    [RelayCommand]
    private void ExportarPdf()
    {
        if (!PuedeExportar)
        {
            _toast.Advertencia("Su rol no tiene permisos de exportación.", "Acceso de solo lectura");
            return;
        }

        if (Ordenes.Count == 0)
        {
            _toast.Advertencia("No hay órdenes que coincidan con los filtros activos.", "Nada para exportar");
            return;
        }

        _exportacion.ExportarOrdenesPdf(TenantId, Ordenes.ToList());
    }
}
