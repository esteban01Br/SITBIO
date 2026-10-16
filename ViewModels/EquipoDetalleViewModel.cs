using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Formulario de edición rápida de la ficha: contenido del diálogo modal.</summary>
public partial class FormularioEdicionEquipo : ObservableObject
{
    [ObservableProperty] private string _estadoSeleccionado = "Operativo";
    [ObservableProperty] private string _ubicacion = string.Empty;
    [ObservableProperty] private string _responsable = string.Empty;

    public IReadOnlyList<string> Estados => EstadosEquipoTexto.Lista;
}

/// <summary>
/// Hoja de vida completa del equipo biomédico (OBJ-03 / OBJ-06): datos de
/// identificación, fechas de mantenimiento y calibración, vida útil, valor de
/// adquisición, campos dinámicos del tenant y trazabilidad de sus órdenes.
/// </summary>
public partial class EquipoDetalleViewModel : ObservableObject
{
    private readonly int _id;
    private readonly ISesionService _sesion;
    private readonly IRepositorioDatos _datos;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;
    private readonly INavegacionService _navegacion;

    public ObservableCollection<OrdenTrabajo> Ordenes { get; } = new();
    public ObservableCollection<SegmentoDistribucion> DistribucionTipos { get; } = new();

    [ObservableProperty] private Equipo? _equipo;
    [ObservableProperty] private bool _noEncontrado;
    [ObservableProperty] private string _contador = "0";
    [ObservableProperty] private string _notaContador = "órdenes abiertas";
    [ObservableProperty] private double _porcentajeVida;
    [ObservableProperty] private string _porcentajeVidaTexto = "0 %";
    [ObservableProperty] private string _vidaTranscurrida = string.Empty;
    [ObservableProperty] private string _vidaUtilTexto = string.Empty;
    [ObservableProperty] private string _valorAdquisicionTexto = "$ 0";
    [ObservableProperty] private string _totalOrdenes = "0";
    [ObservableProperty] private string _notaOrdenes = "sin órdenes registradas";
    [ObservableProperty] private bool _dialogoAbierto;
    [ObservableProperty] private object? _contenidoDialogo;

    /// <summary>Solo los roles con nivel Total sobre Equipos editan la hoja de vida.</summary>
    public bool PuedeEditar => _sesion.PuedeEditar(AreaAplicacion.Equipos);

    public EquipoDetalleViewModel(int id, ISesionService sesion, IRepositorioDatos datos,
        IAuditoriaService auditoria, IToastService toast,
        INavegacionService navegacion, IExportacionService exportacion)
    {
        _id = id;
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

    /// <summary>Carga la ficha del equipo, su hoja de vida y su trazabilidad de órdenes.</summary>
    public void Cargar()
    {
        var equipo = _datos.Equipo(_id);

        // Aislamiento por tenant (RSK-03): no se expone la ficha de otra institución.
        if (equipo is null || equipo.TenantId != TenantId)
        {
            Equipo = null;
            NoEncontrado = true;
            Ordenes.Clear();
            DistribucionTipos.Clear();
            Contador = "0";
            NotaContador = "órdenes abiertas";
            TotalOrdenes = "0";
            NotaOrdenes = "sin órdenes registradas";
            return;
        }

        NoEncontrado = false;
        Equipo = equipo;
        // El modelo Equipo no implementa INotifyPropertyChanged y la referencia no
        // cambia al editar en memoria: se fuerza la relectura de todas las rutas
        // Equipo.* para que los cambios se reflejen de inmediato en la vista.
        OnPropertyChanged(nameof(Equipo));

        // ---------------- Vida útil y valor ----------------
        var mesesTranscurridos = (DateTime.Today - equipo.FechaInstalacion).TotalDays / 30.44;

        PorcentajeVida = equipo.PorcentajeVida;
        PorcentajeVidaTexto = Formateo.Porcentaje(equipo.PorcentajeVida);
        VidaTranscurrida = $"{Formateo.Entero((int)mesesTranscurridos)} meses transcurridos desde la instalación ({Formateo.Fecha(equipo.FechaInstalacion)})";
        VidaUtilTexto = $"vida útil estimada: {Formateo.Entero(equipo.VidaUtilMeses)} meses";
        ValorAdquisicionTexto = Formateo.Dinero(equipo.ValorAdquisicion);

        // ---------------- Órdenes del equipo ----------------
        var ordenes = _datos.OrdenesDe(TenantId)
            .Where(o => o.EquipoId == equipo.Id)
            .OrderByDescending(o => o.FechaSolicitud)
            .ToList();

        Ordenes.Clear();
        foreach (var orden in ordenes) Ordenes.Add(orden);

        var abiertas = ordenes.Count(o => o.EstaAbierta);
        Contador = Formateo.Entero(abiertas);
        NotaContador = abiertas == 1 ? "orden abierta de este equipo" : "órdenes abiertas de este equipo";
        TotalOrdenes = Formateo.Entero(ordenes.Count);
        NotaOrdenes = ordenes.Count == 0
            ? "sin órdenes registradas"
            : ordenes.Count == 1
                ? "1 orden registrada"
                : $"{Formateo.Entero(ordenes.Count)} órdenes registradas";

        // ---------------- Distribución de órdenes por tipo ----------------
        DistribucionTipos.Clear();
        foreach (var grupo in ordenes.GroupBy(o => o.Tipo).OrderByDescending(g => g.Count()))
            DistribucionTipos.Add(new SegmentoDistribucion(EtiquetaTipo(grupo.Key), grupo.Count(), ColorTipo(grupo.Key)));
    }

    private static string EtiquetaTipo(TipoMantenimiento tipo) => tipo switch
    {
        TipoMantenimiento.Preventivo => "Preventivo",
        TipoMantenimiento.Correctivo => "Correctivo",
        TipoMantenimiento.Predictivo => "Predictivo",
        TipoMantenimiento.Calibracion => "Calibración",
        TipoMantenimiento.Instalacion => "Instalación",
        _ => tipo.ToString()
    };

    /// <summary>Colores hexadecimales alineados con el convertidor CvtTipoColor.</summary>
    private static string ColorTipo(TipoMantenimiento tipo) => tipo switch
    {
        TipoMantenimiento.Preventivo => Paleta.InfoHex,
        TipoMantenimiento.Correctivo => Paleta.PeligroHex,
        TipoMantenimiento.Predictivo => "#7A5AF8",
        TipoMantenimiento.Calibracion => Paleta.AdvertenciaHex,
        _ => Paleta.AcentoHex
    };

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void Volver() => _navegacion.Navegar(Rutas.Equipos);

    [RelayCommand]
    private void VerOrden(OrdenTrabajo? orden)
    {
        if (orden is null) return;
        _navegacion.Navegar(Rutas.OrdenDetalle, orden.Id);
    }

    [RelayCommand]
    private void Editar()
    {
        if (Equipo is null) return;

        if (!PuedeEditar)
        {
            _toast.Advertencia("Su rol no tiene nivel de escritura sobre Equipos.", "Acceso de solo lectura");
            return;
        }

        ContenidoDialogo = new FormularioEdicionEquipo
        {
            EstadoSeleccionado = Equipo.EstadoTexto,
            Ubicacion = Equipo.Ubicacion,
            Responsable = Equipo.Responsable
        };
        DialogoAbierto = true;
    }

    [RelayCommand]
    private void Guardar()
    {
        if (ContenidoDialogo is not FormularioEdicionEquipo form) return;

        var equipo = _datos.Equipo(_id);
        if (equipo is null)
        {
            DialogoAbierto = false;
            ContenidoDialogo = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(form.Ubicacion))
        {
            _toast.Advertencia("La ubicación física del equipo no puede quedar vacía.", "Ubicación pendiente");
            return;
        }

        var nuevoEstado = EstadosEquipoTexto.Desde(form.EstadoSeleccionado);
        var nuevaUbicacion = form.Ubicacion.Trim();
        var nuevoResponsable = string.IsNullOrWhiteSpace(form.Responsable)
            ? equipo.Responsable
            : form.Responsable.Trim();

        var cambioEstado = nuevoEstado != equipo.Estado;
        var cambioUbicacion = !equipo.Ubicacion.Equals(nuevaUbicacion, StringComparison.OrdinalIgnoreCase);
        var cambioResponsable = !equipo.Responsable.Equals(nuevoResponsable, StringComparison.OrdinalIgnoreCase);

        if (!cambioEstado && !cambioUbicacion && !cambioResponsable)
        {
            _toast.Informacion("La hoja de vida no tiene cambios pendientes por guardar.", "Sin cambios");
            return;
        }

        equipo.Estado = nuevoEstado;
        equipo.Ubicacion = nuevaUbicacion;
        equipo.Responsable = nuevoResponsable;

        _auditoria.Registrar(AccionAuditoria.Actualizacion, "Equipo", equipo.CodigoInventario,
            $"Actualización de la hoja de vida: estado {equipo.EstadoTexto}, ubicación {equipo.Ubicacion}, responsable {equipo.Responsable}");

        _toast.Exito($"Se actualizó la hoja de vida de {equipo.Nombre}.", "Equipo actualizado");

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
