using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Services;

/// <summary>
/// Contrato de acceso a datos. Hoy lo implementa <see cref="DemoDataProvider"/>
/// (memoria); en la siguiente fase lo implementará Entity Framework Core contra
/// PostgreSQL/SQL Server manteniendo el aislamiento lógico por tenant_id (RSK-03).
/// Todas las consultas filtran por institución.
/// </summary>
public interface IRepositorioDatos
{
    IReadOnlyList<Institucion> Tenants { get; }
    IReadOnlyList<Usuario> Usuarios { get; }
    IReadOnlyList<Equipo> Equipos { get; }
    IReadOnlyList<OrdenTrabajo> Ordenes { get; }
    IReadOnlyList<Tecnico> Tecnicos { get; }
    IReadOnlyList<Intervencion> Intervenciones { get; }
    IReadOnlyList<FormularioDinamico> Formularios { get; }
    IReadOnlyList<EventoAuditoria> Auditoria { get; }
    IReadOnlyList<Notificacion> Notificaciones { get; }

    IReadOnlyList<Usuario> UsuariosDe(int tenantId);
    IReadOnlyList<Equipo> EquiposDe(int tenantId);
    IReadOnlyList<OrdenTrabajo> OrdenesDe(int tenantId);
    IReadOnlyList<Tecnico> TecnicosDe(int tenantId);
    IReadOnlyList<Intervencion> IntervencionesDe(int tenantId);
    IReadOnlyList<FormularioDinamico> FormulariosDe(int tenantId);
    IReadOnlyList<EventoAuditoria> AuditoriaDe(int tenantId);
    IReadOnlyList<Notificacion> NotificacionesDe(int tenantId);

    Equipo? Equipo(int id);
    OrdenTrabajo? Orden(int id);
    OrdenTrabajo? OrdenPorCodigo(string codigo);
    Tecnico? Tecnico(int id);
    Usuario? Usuario(int id);
    FormularioDinamico? Formulario(int id);
    Intervencion? IntervencionDeOrden(int ordenId);

    void AgregarOrden(OrdenTrabajo orden);
    void AgregarEquipo(Equipo equipo);
    void AgregarUsuario(Usuario usuario);
    void AgregarFormulario(FormularioDinamico formulario);
    void AgregarAuditoria(EventoAuditoria evento);
    void AgregarNotificacion(Notificacion notificacion);
    void EliminarEquipo(int id);
    void EliminarFormulario(int id);

    /// <summary>Genera el código único de seguimiento: SIT-2026-000123.</summary>
    string SiguienteCodigoOrden(int tenantId);

    /// <summary>Estadísticas agregadas del tenant para el panel e informes.</summary>
    IndicadoresDashboard Indicadores(int tenantId);
    List<PuntoSerie> SerieMantenimientos(int tenantId, int meses);
    List<SegmentoDistribucion> DistribucionPorEstado(int tenantId);
    List<SegmentoDistribucion> DistribucionPorTipo(int tenantId);
    List<SegmentoDistribucion> DistribucionEquiposPorCategoria(int tenantId);
}
