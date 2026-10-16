namespace SITBIO_Desktop.Core;

/// <summary>
/// Roles del sistema según la matriz de interesados (RBAC) — DOC-01-FCP §6.1.
/// </summary>
public enum RolSistema
{
    Administrador,      // AC-01 / ST-04
    CoordinadorBiomedico, // AC-02 / ST-02
    Tecnico,            // AC-03 / ST-03
    PersonalClinico,    // AC-04 / ST-05
    Director,           // AC-05 / ST-01
    Regulador           // AC-07 / ST-09
}

/// <summary>Áreas del sistema a las que aplica el control de acceso (RBAC).</summary>
public enum AreaAplicacion
{
    Dashboard,
    Equipos,
    Ordenes,
    Intervencion,
    Tecnicos,
    Formularios,
    Reportes,
    Usuarios,
    Configuracion,
    Auditoria
}

/// <summary>Estado operativo del equipo biomédico (hoja de vida).</summary>
public enum EstadoEquipo
{
    Operativo,
    EnMantenimiento,
    EnCalibracion,
    FueraDeServicio,
    Baja
}

/// <summary>
/// Estados del flujo de la orden de trabajo. Se presenta como tablero Kanban
/// (§3 metodología: To Do / In Progress / Code Review / Testing / Done)
/// adaptados al dominio de mantenimiento biomédico.
/// </summary>
public enum EstadoOrden
{
    Nueva,
    Asignada,
    EnProceso,
    EnEsperaRepuesto,
    Verificacion,
    Cerrada,
    Cancelada
}

/// <summary>Columnas del tablero Kanban de órdenes.</summary>
public enum ColumnaKanban
{
    PorHacer,
    EnCurso,
    EnRevision,
    EnPruebas,
    Completada
}

public enum Prioridad
{
    Baja,
    Media,
    Alta,
    Critica
}

public enum TipoMantenimiento
{
    Preventivo,
    Correctivo,
    Predictivo,
    Calibracion,
    Instalacion
}

public enum EstadoIntervencion
{
    Borrador,
    EnEjecucion,
    Finalizada,
    Validada
}

public enum EspecialidadTecnica
{
   ElectronicaBiomedica,
    Mecanica,
    Electroquimica,
    Imagenologia,
    Laboratorio,
    Infusion,
    AireMedico
}

public enum DisponibilidadTecnico
{
    Disponible,
    EnCampo,
    EnDescanso,
    NoDisponible
}

public enum TipoCampoDinamico
{
    Texto,
    Numero,
    Fecha,
    Booleano,
    Seleccion,
    TextoLargo
}

/// <summary>Acciones registradas en la bitácora de auditoría inmutable (RSK-01).</summary>
public enum AccionAuditoria
{
    InicioSesion,
    CierreSesion,
    Creacion,
    Actualizacion,
    Eliminacion,
    Asignacion,
    CambioEstado,
    Exportacion,
    Consulta,
    Validacion
}

/// <summary>Suscripción / plan SaaS del tenant.</summary>
public enum PlanSaaS
{
    Basico,
    Profesional,
    Institucional
}

public enum EstadoTenant
{
    Activo,
    EnPrueba,
    Suspendido
}
