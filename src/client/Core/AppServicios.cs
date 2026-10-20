using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.Core;

/// <summary>
/// Contenedor de servicios de la aplicación. Se inicializa una única vez en el
/// arranque (App.OnStartup). Sustituye esta clase por un contenedor DI real
/// cuando se integre EF Core, sin tocar los ViewModels.
/// </summary>
public static class AppServicios
{
    public static IRepositorioDatos Datos { get; private set; } = null!;
    public static ISesionService Sesion { get; private set; } = null!;
    public static IAuditoriaService Auditoria { get; private set; } = null!;
    public static INavegacionService Navegacion { get; private set; } = null!;
    public static IToastService Toast { get; private set; } = null!;
    public static IExportacionService Exportacion { get; private set; } = null!;

    /// <summary>Modo activo según sitbio.config.json / SITBIO_MODO (fase 2b).</summary>
    public static ModoDatos ModoActivo { get; private set; } = ModoDatos.Demo;

    public static bool Inicializado { get; private set; }

    public static void Inicializar()
    {
        if (Inicializado) return;

        // Interruptor Demo/Api (fase 2b): por defecto Demo, idéntico al comportamiento
        // histórico; "Api" conecta contra SITBIO.Api + PostgreSQL (localhost:5088).
        var cfg = ConfiguracionCliente.Cargar();
        ModoActivo = cfg.Modo;
        Datos = cfg.Modo == ModoDatos.Api
            ? new RepositorioApi(cfg.UrlApi)
            : new DemoDataProvider();

        Sesion = new SesionService(Datos);
        Auditoria = new AuditoriaService(Datos, Sesion);
        Toast = new ToastService();
        Navegacion = new NavegacionService(Sesion, Auditoria);
        Exportacion = new ExportacionService(Datos, Sesion, Auditoria, Toast);

        Inicializado = true;
    }
}
