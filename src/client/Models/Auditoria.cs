using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Bitácora de auditoría inmutable. Es la mitigación del riesgo RSK-01
/// (inconsistencia por registro manual) y soporta auditorías INVIMA / Secretaría
/// de Salud conforme a la Resolución 3100 de 2019.
/// </summary>
public class EventoAuditoria
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public AccionAuditoria Accion { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public string Dispositivo { get; set; } = "Escritorio WPF";

    public string AccionTexto => Accion switch
    {
        AccionAuditoria.InicioSesion => "Inicio de sesión",
        AccionAuditoria.CierreSesion => "Cierre de sesión",
        AccionAuditoria.Creacion => "Creación",
        AccionAuditoria.Actualizacion => "Actualización",
        AccionAuditoria.Eliminacion => "Eliminación",
        AccionAuditoria.Asignacion => "Asignación",
        AccionAuditoria.CambioEstado => "Cambio de estado",
        AccionAuditoria.Exportacion => "Exportación",
        AccionAuditoria.Consulta => "Consulta",
        AccionAuditoria.Validacion => "Validación",
        _ => Accion.ToString()
    };

    public string Icono => Accion switch
    {
        AccionAuditoria.InicioSesion or AccionAuditoria.CierreSesion => "\uE77E",
        AccionAuditoria.Creacion => "\uE710",
        AccionAuditoria.Actualizacion => "\uE70F",
        AccionAuditoria.Eliminacion => "\uE74D",
        AccionAuditoria.Asignacion => "\uE77B",
        AccionAuditoria.CambioEstado => "\uE72C",
        AccionAuditoria.Exportacion => "\uEDE1",
        AccionAuditoria.Consulta => "\uE71E",
        AccionAuditoria.Validacion => "\uE73E",
        _ => "\uE90A"
    };
}

/// <summary>Notificación operativa para el centro de notificaciones.</summary>
public class Notificacion
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Leida { get; set; }
    public string Tipo { get; set; } = "info"; // info | warning | danger | success
    public string Icono { get; set; } = "\uE946";
}
