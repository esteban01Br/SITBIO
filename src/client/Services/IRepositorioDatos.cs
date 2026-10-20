using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Services;

/// <summary>
/// Contrato de acceso a datos. Lo implementan <see cref="DemoDataProvider"/>
/// (memoria, modo Demo) y <see cref="RepositorioApi"/> (API REST + PostgreSQL,
/// modo Api — fase 2b) manteniendo el aislamiento lógico por tenant_id (RSK-03).
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
    void AgregarIntervencion(Intervencion intervencion);
    void AgregarAuditoria(EventoAuditoria evento);
    void AgregarNotificacion(Notificacion notificacion);
    void EliminarEquipo(int id);
    void EliminarFormulario(int id);

    // Escrituras sobre registros existentes (fase 2b: el modo demo las resuelve
    // en memoria; el modo Api hace PUT con la entidad completa).
    void ActualizarOrden(OrdenTrabajo orden);
    void ActualizarEquipo(Equipo equipo);
    void ActualizarUsuario(Usuario usuario);
    void ActualizarIntervencion(Intervencion intervencion);
    void ActualizarFormulario(FormularioDinamico formulario);

    /// <summary>
    /// Valida credenciales del tenant indicado (0 = cualquiera). El modo demo
    /// compara en memoria; el modo Api usa POST auth/login y guarda el JWT.
    /// </summary>
    LoginResultado IntentarAcceso(string email, string clave, int tenantId, out Usuario? usuario);

    /// <summary>
    /// En modo Api la sesión queda atada al tenant del token JWT: el cambio
    /// dinámico de institución solo existe en el modo demo (ADSO-ARQ-01 §6).
    /// </summary>
    bool SoportaCambioTenant => true;

    /// <summary>Genera el código único de seguimiento: SIT-2026-000123.</summary>
    string SiguienteCodigoOrden(int tenantId);

    /// <summary>Estadísticas agregadas del tenant para el panel e informes.</summary>
    IndicadoresDashboard Indicadores(int tenantId);
    List<PuntoSerie> SerieMantenimientos(int tenantId, int meses);
    List<SegmentoDistribucion> DistribucionPorEstado(int tenantId);
    List<SegmentoDistribucion> DistribucionPorTipo(int tenantId);
    List<SegmentoDistribucion> DistribucionEquiposPorCategoria(int tenantId);
}
