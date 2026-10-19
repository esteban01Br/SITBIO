using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Services;

/// <summary>
/// Bitácora de auditoría inmutable (RSK-01). Solo admite escritura, nunca
/// modificación ni borrado, y registra siempre el tenant activo.
/// </summary>
public interface IAuditoriaService
{
    void Registrar(AccionAuditoria accion, string entidad, string referencia, string detalle);
    IReadOnlyList<EventoAuditoria> Consultar(int tenantId);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly IRepositorioDatos _datos;
    private readonly ISesionService _sesion;

    public AuditoriaService(IRepositorioDatos datos, ISesionService sesion)
    {
        _datos = datos;
        _sesion = sesion;
    }

    public void Registrar(AccionAuditoria accion, string entidad, string referencia, string detalle)
    {
        var tenantId = _sesion.TenantActual?.Id ?? 0;

        _datos.AgregarAuditoria(new EventoAuditoria
        {
            TenantId = tenantId,
            Fecha = DateTime.Now,
            Usuario = _sesion.UsuarioActual?.NombreCompleto ?? "Sistema",
            Rol = _sesion.UsuarioActual?.SiglaRol ?? "---",
            Accion = accion,
            Entidad = entidad,
            Referencia = referencia,
            Detalle = detalle,
            Dispositivo = $"Escritorio WPF · {Environment.MachineName}"
        });
    }

    public IReadOnlyList<EventoAuditoria> Consultar(int tenantId) => _datos.AuditoriaDe(tenantId);
}

/// <summary>Alertas efímeras que se muestran como toasts en la aplicación.</summary>
public interface IToastService
{
    event Action<Toast>? ToastSolicitado;
    void Exito(string mensaje, string? titulo = null);
    void Informacion(string mensaje, string? titulo = null);
    void Advertencia(string mensaje, string? titulo = null);
    void Error(string mensaje, string? titulo = null);
}

public record Toast(string Nivel, string Titulo, string Mensaje);

public class ToastService : IToastService
{
    public event Action<Toast>? ToastSolicitado;

    public void Exito(string mensaje, string? titulo = null) => Emitir("exito", titulo ?? "Operación exitosa", mensaje);
    public void Informacion(string mensaje, string? titulo = null) => Emitir("info", titulo ?? "Información", mensaje);
    public void Advertencia(string mensaje, string? titulo = null) => Emitir("warning", titulo ?? "Advertencia", mensaje);
    public void Error(string mensaje, string? titulo = null) => Emitir("error", titulo ?? "Error", mensaje);

    private void Emitir(string nivel, string titulo, string mensaje)
        => ToastSolicitado?.Invoke(new Toast(nivel, titulo, mensaje));
}
