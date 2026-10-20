using System.IO;
using System.Text.Json;

namespace SITBIO_Desktop.Core;

/// <summary>Modo de acceso a datos de la aplicación (fase 2b).</summary>
public enum ModoDatos
{
    /// <summary>Datos demo en memoria (DemoDataProvider): no requiere servicios.</summary>
    Demo,

    /// <summary>API REST SITBIO (RepositorioApi): requiere API + PostgreSQL.</summary>
    Api
}

/// <summary>
/// Configuración del cliente, leída de <c>sitbio.config.json</c> junto al ejecutable.
/// Las variables de entorno <c>SITBIO_MODO</c> (Demo/Api) y <c>SITBIO_URL_API</c>
/// tienen prioridad, lo que permite probar el modo API sin editar archivos
/// (SITBIO_MODO=Api dotnet run --project src/client/SITBIO-Desktop.csproj).
/// Si algo falla se usa la configuración por defecto: modo Demo.
/// </summary>
public sealed class ConfiguracionCliente
{
    public ModoDatos Modo { get; init; } = ModoDatos.Demo;
    public string UrlApi { get; init; } = "http://localhost:5088";

    public static ConfiguracionCliente Cargar()
    {
        var modo = ModoDatos.Demo;
        var url = "http://localhost:5088";

        try
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "sitbio.config.json");
            if (File.Exists(ruta))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(ruta));
                var raiz = doc.RootElement;

                if (raiz.TryGetProperty("ModoDatos", out var m) &&
                    string.Equals(m.GetString(), "Api", StringComparison.OrdinalIgnoreCase))
                    modo = ModoDatos.Api;

                if (raiz.TryGetProperty("UrlApi", out var u) &&
                    !string.IsNullOrWhiteSpace(u.GetString()))
                    url = u.GetString()!.TrimEnd('/');
            }
        }
        catch
        {
            // Archivo ausente o ilegible: se conserva la configuración por defecto (Demo).
        }

        var envModo = Environment.GetEnvironmentVariable("SITBIO_MODO");
        if (!string.IsNullOrWhiteSpace(envModo))
            modo = string.Equals(envModo, "Api", StringComparison.OrdinalIgnoreCase)
                ? ModoDatos.Api
                : ModoDatos.Demo;

        var envUrl = Environment.GetEnvironmentVariable("SITBIO_URL_API");
        if (!string.IsNullOrWhiteSpace(envUrl))
            url = envUrl.TrimEnd('/');

        return new ConfiguracionCliente { Modo = modo, UrlApi = url };
    }
}
