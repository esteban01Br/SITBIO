namespace SITBIO.Api.Domain;

/// <summary>
/// Espejo exacto de src/client/Core/Enums.cs. Los valores numéricos DEBEN coincidir
/// con los del cliente WPF porque el JSON serializa los enums como números.
/// </summary>
public enum RolSistema
{
    Administrador,
    CoordinadorBiomedico,
    Tecnico,
    PersonalClinico,
    Director,
    Regulador
}

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

public enum EstadoEquipo
{
    Operativo,
    EnMantenimiento,
    EnCalibracion,
    FueraDeServicio,
    Baja
}

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

/// <summary>Acciones de la bitácora inmutable (RSK-01). La tabla solo admite INSERT.</summary>
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
