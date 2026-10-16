using System.Windows;
using System.Windows.Threading;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Services;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Cultura de la aplicación: fechas, números y moneda en es-CO de forma
        // consistente entre la interfaz, la exportación y la bitácora.
        var cultura = new System.Globalization.CultureInfo("es-CO");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultura;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultura;

        // Servicios de la aplicación (se sustituyen por EF Core en la fase de BD).
        AppServicios.Inicializar();
        RegistrarRutas();

        DispatcherUnhandledException += AlErrorNoControlado;
    }

    /// <summary>Convierte el parámetro de navegación en el id que esperan los detalles.</summary>
    private static int IdDe(object? parametro) => parametro switch
    {
        int id => id,
        Models.OrdenTrabajo orden => orden.Id,
        Models.Equipo equipo => equipo.Id,
        Models.Usuario usuario => usuario.Id,
        Models.FormularioDinamico formulario => formulario.Id,
        string texto when int.TryParse(texto, out var id) => id,
        _ => 0
    };

    /// <summary>
    /// Fábricas de las pantallas. Cada clave de `Rutas` se asocia aquí con su
    /// ViewModel; la vista se resuelve por el DataTemplate de App.xaml.
    /// </summary>
    private static void RegistrarRutas()
    {
        var nav = (NavegacionService)AppServicios.Navegacion;
        var datos = AppServicios.Datos;
        var sesion = AppServicios.Sesion;
        var auditoria = AppServicios.Auditoria;
        var toast = AppServicios.Toast;
        var exportacion = AppServicios.Exportacion;

        nav.Registrar(Rutas.Dashboard, _ =>
            new DashboardViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        // ---- Operación: bandeja, detalle e intervención ----
        nav.Registrar(Rutas.Ordenes, _ =>
            new OrdenesViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.OrdenDetalle, parametro =>
            new OrdenDetalleViewModel(IdDe(parametro), sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.Intervencion, parametro =>
            new IntervencionViewModel(IdDe(parametro), sesion, datos, auditoria, toast, nav, exportacion));

        // ---- Activos biomédicos ----
        nav.Registrar(Rutas.Equipos, _ =>
            new EquiposViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.EquipoDetalle, parametro =>
            new EquipoDetalleViewModel(IdDe(parametro), sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.Tecnicos, _ =>
            new TecnicosViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        // ---- Analítica, formularios y usuarios ----
        nav.Registrar(Rutas.Reportes, _ =>
            new ReportesViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.Formularios, _ =>
            new FormulariosViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.FormBuilder, parametro =>
            new FormBuilderViewModel(IdDe(parametro), sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.Usuarios, _ =>
            new UsuariosViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.UsuarioDetalle, parametro =>
            new UsuarioDetalleViewModel(IdDe(parametro), sesion, datos, auditoria, toast, nav, exportacion));

        // ---- Sistema ----
        nav.Registrar(Rutas.Configuracion, _ =>
            new ConfiguracionViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.Auditoria, _ =>
            new AuditoriaViewModel(sesion, datos, auditoria, toast, nav, exportacion));

        nav.Registrar(Rutas.Calibraciones, _ => new ProximamenteViewModel("calibraciones"));
        nav.Registrar(Rutas.Inventario, _ => new ProximamenteViewModel("inventario"));
    }

    private void AlErrorNoControlado(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Ocurrió un error inesperado.\n\n{e.Exception.Message}",
            "SITBIO — Error de la aplicación",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
