using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Services;

/// <summary>
/// Implementación de <see cref="IRepositorioDatos"/> contra la API REST SITBIO
/// (fase 2b): HTTP a la URL configurada con el JWT de <c>auth/login</c>.
///
/// Decisiones:
/// <list type="bullet">
/// <item>La interfaz es síncrona; las llamadas HTTP se ejecutan en Task.Run y se
/// bloquean (localhost responde en milisegundos y no se captura el hilo UI).</item>
/// <item>Las propiedades de colección se cachean por sesión y se invalidan en cada
/// escritura; el servidor ya acota por tenant vía el claim JWT, por lo que
/// <c>*De(tenantId)</c> devuelve vacío si el tenant no coincide con la sesión.</item>
/// <item>Los errores no lanzan (paridad con el demo): se avisan con toast y se
/// mantiene la UI operativa.</item>
/// </list>
/// </summary>
public sealed class RepositorioApi : IRepositorioDatos
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private int? _tenantSesion;
    private DateTime _ultimoAviso = DateTime.MinValue;

    private List<Institucion>? _tenants;
    private List<Usuario>? _usuarios;
    private List<Equipo>? _equipos;
    private List<OrdenTrabajo>? _ordenes;
    private List<Tecnico>? _tecnicos;
    private List<Intervencion>? _intervenciones;
    private List<FormularioDinamico>? _formularios;
    private List<EventoAuditoria>? _auditoria;
    private List<Notificacion>? _notificaciones;

    public RepositorioApi(string urlBase)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(urlBase.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    /// <summary>El token JWT está atado a un tenant: no se admite cambio dinámico.</summary>
    public bool SoportaCambioTenant => false;

    // ============================================================
    //                      COLECCIONES (con caché)
    // ============================================================
    public IReadOnlyList<Institucion> Tenants => Cargar(ref _tenants, "api/v1/tenants");
    public IReadOnlyList<Usuario> Usuarios => Cargar(ref _usuarios, "api/v1/usuarios");
    public IReadOnlyList<Equipo> Equipos => Cargar(ref _equipos, "api/v1/equipos");
    public IReadOnlyList<OrdenTrabajo> Ordenes => Cargar(ref _ordenes, "api/v1/ordenes");
    public IReadOnlyList<Tecnico> Tecnicos => Cargar(ref _tecnicos, "api/v1/tecnicos");
    public IReadOnlyList<Intervencion> Intervenciones => Cargar(ref _intervenciones, "api/v1/intervenciones");
    public IReadOnlyList<FormularioDinamico> Formularios => Cargar(ref _formularios, "api/v1/formularios");
    public IReadOnlyList<EventoAuditoria> Auditoria => Cargar(ref _auditoria, "api/v1/auditoria");
    public IReadOnlyList<Notificacion> Notificaciones => Cargar(ref _notificaciones, "api/v1/notificaciones");

    public IReadOnlyList<Usuario> UsuariosDe(int tenantId) =>
        FueraDeSesion(tenantId) ? new List<Usuario>() : Cargar(ref _usuarios, "api/v1/usuarios");

    public IReadOnlyList<Equipo> EquiposDe(int tenantId) =>
        FueraDeSesion(tenantId) ? new List<Equipo>() : Cargar(ref _equipos, "api/v1/equipos");

    public IReadOnlyList<OrdenTrabajo> OrdenesDe(int tenantId) =>
        FueraDeSesion(tenantId) ? new List<OrdenTrabajo>() : Cargar(ref _ordenes, "api/v1/ordenes");

    public IReadOnlyList<Tecnico> TecnicosDe(int tenantId) =>
        FueraDeSesion(tenantId) ? new List<Tecnico>() : Cargar(ref _tecnicos, "api/v1/tecnicos");

    public IReadOnlyList<Intervencion> IntervencionesDe(int tenantId) =>
        FueraDeSesion(tenantId) ? new List<Intervencion>() : Cargar(ref _intervenciones, "api/v1/intervenciones");

    public IReadOnlyList<FormularioDinamico> FormulariosDe(int tenantId) =>
        FueraDeSesion(tenantId) ? new List<FormularioDinamico>() : Cargar(ref _formularios, "api/v1/formularios");

    public IReadOnlyList<EventoAuditoria> AuditoriaDe(int tenantId) =>
        FueraDeSesion(tenantId)
            ? new List<EventoAuditoria>()
            : Cargar(ref _auditoria, "api/v1/auditoria").OrderByDescending(x => x.Fecha).ToList();

    public IReadOnlyList<Notificacion> NotificacionesDe(int tenantId) =>
        FueraDeSesion(tenantId)
            ? new List<Notificacion>()
            : Cargar(ref _notificaciones, "api/v1/notificaciones").OrderByDescending(x => x.Fecha).ToList();

    // ============================================================
    //                        PUNTUALES
    // ============================================================
    public Equipo? Equipo(int id) => Equipos.FirstOrDefault(x => x.Id == id);
    public OrdenTrabajo? Orden(int id) => Ordenes.FirstOrDefault(x => x.Id == id);

    public OrdenTrabajo? OrdenPorCodigo(string codigo) =>
        Ordenes.FirstOrDefault(x => x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));

    public Tecnico? Tecnico(int id) => Tecnicos.FirstOrDefault(x => x.Id == id);
    public Usuario? Usuario(int id) => Usuarios.FirstOrDefault(x => x.Id == id);
    public FormularioDinamico? Formulario(int id) => Formularios.FirstOrDefault(x => x.Id == id);
    public Intervencion? IntervencionDeOrden(int ordenId) => Intervenciones.FirstOrDefault(x => x.OrdenId == ordenId);

    public string SiguienteCodigoOrden(int tenantId)
    {
        var codigo = Obtener<string>("api/v1/ordenes/siguiente-codigo");
        return codigo ?? $"SIT-{DateTime.Today.Year}-{(Ordenes.Count(x => x.TenantId == tenantId) + 1):D6}";
    }

    // ============================================================
    //                      ESCRITURAS
    // ============================================================
    public void AgregarOrden(OrdenTrabajo orden)
    {
        var creado = Post("api/v1/ordenes", orden, "crear la orden de trabajo");
        if (creado is not null) orden.Id = creado.Id;
    }

    public void AgregarEquipo(Equipo equipo)
    {
        var creado = Post("api/v1/equipos", equipo, "crear el equipo");
        if (creado is not null) equipo.Id = creado.Id;
    }

    public void AgregarUsuario(Usuario usuario)
    {
        var creado = Post("api/v1/usuarios", usuario, "crear la cuenta de usuario");
        if (creado is not null) usuario.Id = creado.Id;
    }

    public void AgregarFormulario(FormularioDinamico formulario)
    {
        var creado = Post("api/v1/formularios", formulario, "crear el formulario");
        if (creado is not null) formulario.Id = creado.Id;
    }

    public void AgregarIntervencion(Intervencion intervencion)
    {
        var creado = Post("api/v1/intervenciones", intervencion, "abrir la ficha de intervención");
        if (creado is not null) intervencion.Id = creado.Id;
    }

    public void AgregarAuditoria(EventoAuditoria evento) =>
        Post("api/v1/auditoria", evento, "registrar el evento de auditoría");

    public void AgregarNotificacion(Notificacion notificacion) =>
        Post("api/v1/notificaciones", notificacion, "crear la notificación");

    public void ActualizarOrden(OrdenTrabajo orden) =>
        Put($"api/v1/ordenes/{orden.Id}", orden, "guardar la orden de trabajo");

    public void ActualizarEquipo(Equipo equipo) =>
        Put($"api/v1/equipos/{equipo.Id}", equipo, "guardar el equipo");

    public void ActualizarUsuario(Usuario usuario) =>
        Put($"api/v1/usuarios/{usuario.Id}", usuario, "guardar la cuenta de usuario");

    public void ActualizarIntervencion(Intervencion intervencion) =>
        Put($"api/v1/intervenciones/{intervencion.Id}", intervencion, "guardar la intervención");

    public void ActualizarFormulario(FormularioDinamico formulario) =>
        Put($"api/v1/formularios/{formulario.Id}", formulario, "guardar el formulario");

    public void EliminarEquipo(int id) => Eliminar($"api/v1/equipos/{id}", "eliminar el equipo");

    public void EliminarFormulario(int id) => Eliminar($"api/v1/formularios/{id}", "eliminar el formulario");

    // ============================================================
    //                          ACCESO
    // ============================================================
    /// <summary>POST /api/v1/auth/login; guarda el JWT para el resto de la sesión.</summary>
    public LoginResultado IntentarAcceso(string email, string clave, int tenantId, out Usuario? usuario)
    {
        usuario = null;
        try
        {
            var resultado = Ejecutar(async () =>
            {
                using var resp = await _http.PostAsJsonAsync("api/v1/auth/login",
                    new { email, clave, tenantId }, Json);

                // 403: cuenta desactivada (distinguido por la API, paridad con el demo).
                if (resp.StatusCode == HttpStatusCode.Forbidden)
                    return (LoginResultado.UsuarioInactivo, (Usuario?)null);

                if (resp.StatusCode == HttpStatusCode.BadRequest)
                    return (LoginResultado.CredencialesInvalidas, (Usuario?)null);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // La API marca {"codigo":"tenant"} cuando el usuario no pertenece
                    // a la institución seleccionada.
                    var codigo = ExtraerCodigo(resp);
                    return codigo == "tenant"
                        ? (LoginResultado.TenantInvalido, (Usuario?)null)
                        : (LoginResultado.CredencialesInvalidas, (Usuario?)null);
                }

                if (!resp.IsSuccessStatusCode)
                {
                    AvisarError(resp, "iniciar sesión");
                    return (LoginResultado.CredencialesInvalidas, (Usuario?)null);
                }

                var login = await resp.Content.ReadFromJsonAsync<RespuestaLogin>(Json);
                if (login?.Usuario is null || string.IsNullOrEmpty(login.Token))
                    return (LoginResultado.CredencialesInvalidas, (Usuario?)null);

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", login.Token);
                _tenantSesion = login.Usuario.TenantId;
                LimpiarCache();
                return (LoginResultado.Correcto, login.Usuario);
            });

            usuario = resultado.Item2;
            return resultado.Item1;
        }
        catch (Exception ex)
        {
            AvisarFallo(ex, "iniciar sesión");
            return LoginResultado.CredencialesInvalidas;
        }
    }

    private sealed record RespuestaLogin(string Token, DateTime ExpiraEn, Usuario? Usuario);

    // ============================================================
    //                     ESTADÍSTICAS (dashboard)
    // ============================================================
    public IndicadoresDashboard Indicadores(int tenantId) =>
        FueraDeSesion(tenantId)
            ? new IndicadoresDashboard()
            : Obtener<IndicadoresDashboard>("api/v1/dashboard/indicadores") ?? new IndicadoresDashboard();

    public List<PuntoSerie> SerieMantenimientos(int tenantId, int meses) =>
        FueraDeSesion(tenantId)
            ? new List<PuntoSerie>()
            : Obtener<List<PuntoSerie>>($"api/v1/dashboard/serie?meses={meses}") ?? new List<PuntoSerie>();

    public List<SegmentoDistribucion> DistribucionPorEstado(int tenantId) =>
        FueraDeSesion(tenantId)
            ? new List<SegmentoDistribucion>()
            : Obtener<List<SegmentoDistribucion>>("api/v1/dashboard/distribucion-estado") ?? new List<SegmentoDistribucion>();

    public List<SegmentoDistribucion> DistribucionPorTipo(int tenantId) =>
        FueraDeSesion(tenantId)
            ? new List<SegmentoDistribucion>()
            : Obtener<List<SegmentoDistribucion>>("api/v1/dashboard/distribucion-tipo") ?? new List<SegmentoDistribucion>();

    public List<SegmentoDistribucion> DistribucionEquiposPorCategoria(int tenantId) =>
        FueraDeSesion(tenantId)
            ? new List<SegmentoDistribucion>()
            : Obtener<List<SegmentoDistribucion>>("api/v1/dashboard/distribucion-equipos") ?? new List<SegmentoDistribucion>();

    // ============================================================
    //                    AUXILIARES DE RED
    // ============================================================
    private IReadOnlyList<T> Cargar<T>(ref List<T>? cache, string ruta) where T : class
    {
        if (cache is null)
            cache = Obtener<List<T>>(ruta) ?? new List<T>();
        return cache;
    }

    private bool FueraDeSesion(int tenantId) => _tenantSesion is int activo && activo != tenantId;

    private void LimpiarCache()
    {
        _usuarios = null;
        _equipos = null;
        _ordenes = null;
        _tecnicos = null;
        _intervenciones = null;
        _formularios = null;
        _auditoria = null;
        _notificaciones = null;
    }

    /// <summary>GET; devuelve null si la API responde 404 o falla (con aviso).</summary>
    private T? Obtener<T>(string ruta) where T : class
    {
        try
        {
            return Ejecutar(async () =>
            {
                using var resp = await _http.GetAsync(ruta);
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                if (!resp.IsSuccessStatusCode)
                {
                    AvisarError(resp, "consultar datos");
                    return null;
                }
                return await resp.Content.ReadFromJsonAsync<T>(Json);
            });
        }
        catch (Exception ex)
        {
            AvisarFallo(ex, "consultar datos");
            return null;
        }
    }

    /// <summary>POST; devuelve la entidad creada (para copiar el Id) o null.</summary>
    private T? Post<T>(string ruta, T cuerpo, string operacion) where T : class
    {
        try
        {
            var creado = Ejecutar(async () =>
            {
                using var resp = await _http.PostAsJsonAsync(ruta, cuerpo, Json);
                if (!resp.IsSuccessStatusCode)
                {
                    AvisarError(resp, operacion);
                    return null;
                }
                return await resp.Content.ReadFromJsonAsync<T>(Json);
            });
            if (creado is not null) LimpiarCache();
            return creado;
        }
        catch (Exception ex)
        {
            AvisarFallo(ex, operacion);
            return null;
        }
    }

    /// <summary>PUT de reemplazo total (el servidor trabaja con la entidad completa).</summary>
    private void Put<T>(string ruta, T cuerpo, string operacion)
    {
        try
        {
            var ok = Ejecutar(async () =>
            {
                using var resp = await _http.PutAsJsonAsync(ruta, cuerpo, Json);
                if (resp.IsSuccessStatusCode) return true;
                AvisarError(resp, operacion);
                return false;
            });
            if (ok) LimpiarCache();
        }
        catch (Exception ex)
        {
            AvisarFallo(ex, operacion);
        }
    }

    private void Eliminar(string ruta, string operacion)
    {
        try
        {
            var ok = Ejecutar(async () =>
            {
                using var resp = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, ruta));
                if (resp.IsSuccessStatusCode) return true;
                AvisarError(resp, operacion);
                return false;
            });
            if (ok) LimpiarCache();
        }
        catch (Exception ex)
        {
            AvisarFallo(ex, operacion);
        }
    }

    /// <summary>Bloquea fuera del hilo UI sin capturar el SynchronizationContext (WPF).</summary>
    private static T Ejecutar<T>(Func<Task<T>> accion) =>
        Task.Run(accion).GetAwaiter().GetResult();

    // ============================================================
    //                    AVISOS (sin lanzar excepciones)
    // ============================================================
    private void AvisarError(HttpResponseMessage resp, string operacion)
    {
        var detalle = string.Empty;
        try
        {
            var cuerpo = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(cuerpo);
            if (doc.RootElement.TryGetProperty("mensaje", out var m))
                detalle = m.GetString() ?? string.Empty;
        }
        catch
        {
            // Cuerpo no JSON: se usa solo el código HTTP.
        }

        Avisar(string.IsNullOrWhiteSpace(detalle)
            ? $"No se pudo {operacion} (HTTP {(int)resp.StatusCode})."
            : $"No se pudo {operacion}: {detalle}");
    }

    private void AvisarFallo(Exception ex, string operacion) =>
        Avisar($"No se pudo {operacion}: {ex.Message}. Verifique que la API esté activa en {_http.BaseAddress}.");

    private void Avisar(string mensaje)
    {
        // Evita ráfagas de toasts si la API se cae (una advertencia cada 8 s).
        if ((DateTime.Now - _ultimoAviso).TotalSeconds < 8) return;
        _ultimoAviso = DateTime.Now;
        AppServicios.Toast?.Advertencia(mensaje, "Conexión con la API");
    }

    private static string ExtraerCodigo(HttpResponseMessage resp)
    {
        try
        {
            var cuerpo = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(cuerpo);
            if (doc.RootElement.TryGetProperty("codigo", out var c))
                return c.GetString() ?? string.Empty;
        }
        catch
        {
            // Sin cuerpo o no JSON.
        }
        return string.Empty;
    }
}
