using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>
/// Etiquetas en español de <see cref="EstadoEquipo"/> que usan los selectores
/// de estado de los módulos de Equipos y EquipoDetalle.
/// </summary>
public static class EstadosEquipoTexto
{
    public static readonly IReadOnlyList<string> Lista = new[]
    {
        "Operativo", "En mantenimiento", "En calibración", "Fuera de servicio", "Dado de baja"
    };

    /// <summary>Convierte la etiqueta elegida en el selector a su valor de enum.</summary>
    public static EstadoEquipo Desde(string? texto) => texto switch
    {
        "En mantenimiento" => EstadoEquipo.EnMantenimiento,
        "En calibración" => EstadoEquipo.EnCalibracion,
        "Fuera de servicio" => EstadoEquipo.FueraDeServicio,
        "Dado de baja" => EstadoEquipo.Baja,
        _ => EstadoEquipo.Operativo
    };
}

/// <summary>Formulario de alta de equipo: es el contenido del diálogo modal.</summary>
public partial class FormularioEquipo : ObservableObject
{
    [ObservableProperty] private string _codigoInventario = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _categoria = string.Empty;
    [ObservableProperty] private string _marca = string.Empty;
    [ObservableProperty] private string _modelo = string.Empty;
    [ObservableProperty] private string _numeroSerie = string.Empty;
    [ObservableProperty] private string _ubicacion = string.Empty;
    [ObservableProperty] private string _responsable = string.Empty;
    [ObservableProperty] private string _estadoSeleccionado = "Operativo";

    public IReadOnlyList<string> Estados => EstadosEquipoTexto.Lista;
}

/// <summary>
/// Módulo de equipos (inventario biomédico): parque por institución con alta,
/// búsqueda, filtros y baja auditadas. Es la hoja de vida del activo (OBJ-03).
/// </summary>
public partial class EquiposViewModel : ObservableObject
{
    private const string TodasLasCategorias = "Todas las categorías";
    private const string TodosLosEstados = "Todos los estados";

    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;
    private readonly IExportacionService _exportacion;

    private Equipo? _equipoAEliminar;

    public ObservableCollection<Equipo> Equipos { get; } = new();
    public ObservableCollection<TarjetaKpi> Kpis { get; } = new();
    public ObservableCollection<string> CategoriasFiltro { get; } = new();
    public ObservableCollection<string> EstadosFiltro { get; } = new();

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private string _categoriaFiltro = TodasLasCategorias;
    [ObservableProperty] private string _estadoFiltro = TodosLosEstados;
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "equipos en el parque biomédico";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;
    [ObservableProperty] private bool _confirmacionAbierta;
    [ObservableProperty] private string _mensajeConfirmacion = string.Empty;

    /// <summary>Solo los roles con nivel Total sobre Equipos escriben el inventario.</summary>
    public bool PuedeEditar => _sesion.PuedeEditar(AreaAplicacion.Equipos);

    public EquiposViewModel(ISesionService sesion, IRepositorioDatos datos,
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
        foreach (var etiqueta in EstadosEquipoTexto.Lista)
            EstadosFiltro.Add(etiqueta);

        Cargar();
    }

    private int TenantId => _sesion.TenantActual?.Id ?? 0;

    /// <summary>Recalcula los indicadores, refresca las categorías y reaplica los filtros.</summary>
    public void Cargar()
    {
        var equipos = _datos.EquiposDe(TenantId).ToList();

        var total = equipos.Count;
        var operativos = equipos.Count(e => e.Estado == EstadoEquipo.Operativo);
        var detenidos = equipos.Count(e => e.Estado is EstadoEquipo.EnMantenimiento or EstadoEquipo.FueraDeServicio);
        var enCalibracion = equipos.Count(e => e.Estado == EstadoEquipo.EnCalibracion);
        var vencidos = equipos.Count(e => e.PreventivoVencido);
        var disponibilidad = total == 0 ? 100d : operativos / (double)total * 100;
        var categorias = equipos.Select(e => e.Categoria).Distinct().Count();

        // ---------------- Tarjetas de indicadores ----------------
        Kpis.Clear();

        Kpis.Add(TarjetaKpi.Crear(
            "Parque biomédico registrado",
            Formateo.Entero(total),
            "\uE912", "info",
            delta: $"{Formateo.Entero(categorias)} categorías",
            deltaIcono: "\uE8A7",
            leyenda: "activos en la institución",
            ayuda: "Inventario aislado por la institución activa (RSK-03). Cada fila abre la hoja de vida completa del equipo."));

        Kpis.Add(TarjetaKpi.Crear(
            "Equipos operativos",
            Formateo.Entero(operativos),
            "\uE73E", disponibilidad >= 90 ? "exito" : "advertencia",
            delta: Formateo.Porcentaje(disponibilidad) + " de disponibilidad",
            deltaIcono: "\uE74A",
            leyenda: "parque disponible",
            ayuda: "Equipos en estado Operativo frente al total registrado; es el indicador de disponibilidad del OBJ-04."));

        Kpis.Add(TarjetaKpi.Crear(
            "En mantenimiento o fuera de servicio",
            Formateo.Entero(detenidos),
            "\uE895", detenidos > 0 ? "advertencia" : "exito",
            delta: enCalibracion > 0 ? $"{Formateo.Entero(enCalibracion)} en calibración" : "sin equipos en calibración",
            deltaIcono: "\uE823",
            leyenda: "detenidos o en taller",
            ayuda: "Equipos que no prestan servicio clínico mientras se atiende el mantenimiento correctivo o preventivo."));

        Kpis.Add(TarjetaKpi.Crear(
            "Preventivos vencidos",
            Formateo.Entero(vencidos),
            "\uE787", vencidos > 0 ? "peligro" : "exito",
            delta: vencidos > 0 ? "requiere programación" : "todo al día",
            deltaIcono: vencidos > 0 ? "\uE7BA" : "\uE73E",
            leyenda: "fuera de fecha",
            ayuda: "Equipos cuya fecha programada de mantenimiento preventivo ya venció; requieren reprogramación inmediata."));

        Contador = Formateo.Entero(total);
        NotaContador = "equipos en el parque biomédico";

        // ---------------- Categorías del filtro ----------------
        var categoriaActual = CategoriaFiltro;
        CategoriasFiltro.Clear();
        CategoriasFiltro.Add(TodasLasCategorias);
        foreach (var categoria in equipos
                     .Select(e => e.Categoria)
                     .Distinct()
                     .OrderBy(c => c, StringComparer.CurrentCultureIgnoreCase))
            CategoriasFiltro.Add(categoria);

        if (!CategoriasFiltro.Contains(categoriaActual))
            CategoriaFiltro = TodasLasCategorias;

        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var equipos = _datos.EquiposDe(TenantId).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim();
            equipos = equipos.Where(e =>
                e.CodigoInventario.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Marca.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Modelo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                e.Ubicacion.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (CategoriaFiltro != TodasLasCategorias)
            equipos = equipos.Where(e => e.Categoria == CategoriaFiltro);

        if (EstadoFiltro != TodosLosEstados)
            equipos = equipos.Where(e => e.EstadoTexto == EstadoFiltro);

        Equipos.Clear();
        var filtrados = equipos
            .OrderBy(x => x.CodigoInventario, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        foreach (var equipo in filtrados)
            Equipos.Add(equipo);
    }

    partial void OnBusquedaChanged(string value) => AplicarFiltros();
    partial void OnCategoriaFiltroChanged(string value) => AplicarFiltros();
    partial void OnEstadoFiltroChanged(string value) => AplicarFiltros();

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void LimpiarFiltros()
    {
        Busqueda = string.Empty;
        CategoriaFiltro = TodasLasCategorias;
        EstadoFiltro = TodosLosEstados;
        _toast.Informacion("Se restablecieron los filtros del inventario biomédico.", "Filtros limpiados");
    }

    [RelayCommand]
    private void Ver(Equipo? equipo)
    {
        if (equipo is null) return;
        _navegacion.Navegar(Rutas.EquipoDetalle, equipo.Id);
    }

    [RelayCommand]
    private void Nuevo()
    {
        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Equipos.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioEquipo();
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (ContenidoDialogo is not FormularioEquipo form) return;

        if (string.IsNullOrWhiteSpace(form.CodigoInventario))
        {
            _toast.Advertencia("Ingrese el código de inventario del equipo (ej. BM-0231).", "Código pendiente");
            return;
        }

        if (string.IsNullOrWhiteSpace(form.Nombre))
        {
            _toast.Advertencia("Ingrese el nombre o la descripción técnica del equipo.", "Nombre pendiente");
            return;
        }

        if (string.IsNullOrWhiteSpace(form.Categoria))
        {
            _toast.Advertencia("Ingrese la categoría del equipo (ej. Bomba de infusión).", "Categoría pendiente");
            return;
        }

        if (_datos.EquiposDe(TenantId).Any(e =>
                e.CodigoInventario.Equals(form.CodigoInventario.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            _toast.Error("Ya existe un equipo con ese código de inventario en esta institución.", "Código duplicado");
            return;
        }

        var equipo = new Equipo
        {
            TenantId = TenantId,
            CodigoInventario = form.CodigoInventario.Trim().ToUpperInvariant(),
            Nombre = form.Nombre.Trim(),
            Categoria = form.Categoria.Trim(),
            Marca = string.IsNullOrWhiteSpace(form.Marca) ? "Por definir" : form.Marca.Trim(),
            Modelo = string.IsNullOrWhiteSpace(form.Modelo) ? "Por definir" : form.Modelo.Trim(),
            NumeroSerie = string.IsNullOrWhiteSpace(form.NumeroSerie) ? "SN pendiente" : form.NumeroSerie.Trim(),
            Ubicacion = form.Ubicacion.Trim(),
            Responsable = string.IsNullOrWhiteSpace(form.Responsable) ? "Ingeniería biomédica" : form.Responsable.Trim(),
            Estado = EstadosEquipoTexto.Desde(form.EstadoSeleccionado),
            FechaInstalacion = DateTime.Today,
            UltimoMantenimiento = null,
            ProximoMantenimiento = DateTime.Today.AddDays(90),
            UltimaCalibracion = null,
            ProximaCalibracion = DateTime.Today.AddDays(365),
            VidaUtilMeses = 84,
            ValorAdquisicion = 0,
            Fabricante = string.IsNullOrWhiteSpace(form.Marca) ? "Por definir" : form.Marca.Trim(),
            ClaseRiesgo = "II",
            ResolucionAplicable = "Res. 3100/2019",
            OrdenesAbiertas = 0,
            TotalIntervenciones = 0
        };

        _datos.AgregarEquipo(equipo);

        _auditoria.Registrar(AccionAuditoria.Creacion, "Equipo", equipo.CodigoInventario,
            $"Alta de {equipo.Nombre} ({equipo.Categoria}) en {equipo.Ubicacion}");

        _toast.Exito($"Se registró {equipo.Nombre} con el código {equipo.CodigoInventario}.", "Equipo creado");

        DialogoAbierto = false;
        ContenidoDialogo = null;
        Cargar();
    }

    [RelayCommand]
    private void Eliminar(Equipo? equipo)
    {
        if (equipo is null) return;

        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Equipos.", "Acceso de solo lectura");
            return;
        }

        _equipoAEliminar = equipo;
        MensajeConfirmacion =
            $"El equipo {equipo.CodigoInventario} — {equipo.Nombre} dejará de estar disponible en el inventario " +
            $"de la institución y no podrá recibir nuevas órdenes de trabajo. " +
            "La eliminación queda registrada de forma permanente en la bitácora de auditoría.";
        ConfirmacionAbierta = true;
    }

    [RelayCommand]
    private void ConfirmarEliminacion()
    {
        if (_equipoAEliminar is null)
        {
            ConfirmacionAbierta = false;
            return;
        }

        var equipo = _equipoAEliminar;
        _equipoAEliminar = null;

        _datos.EliminarEquipo(equipo.Id);

        _auditoria.Registrar(AccionAuditoria.Eliminacion, "Equipo", equipo.CodigoInventario,
            $"Baja del inventario de {equipo.Nombre} ({equipo.Categoria})");

        _toast.Exito($"Se retiró {equipo.Nombre} ({equipo.CodigoInventario}) del inventario.", "Equipo eliminado");

        ConfirmacionAbierta = false;
        Cargar();
    }

    [RelayCommand]
    private void Cancelar()
    {
        DialogoAbierto = false;
        ContenidoDialogo = null;
        ConfirmacionAbierta = false;
        _equipoAEliminar = null;
    }

    [RelayCommand]
    private void ExportarExcel()
    {
        if (TenantId == 0) return;
        // El servicio de exportación registra la auditoría y el toast internamente.
        _exportacion.ExportarInventarioExcel(TenantId);
    }
}
