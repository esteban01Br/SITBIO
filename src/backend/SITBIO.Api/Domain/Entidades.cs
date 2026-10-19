using System.Text.Json.Serialization;

namespace SITBIO.Api.Domain;

// Entidades de persistencia. Cada propiedad coincide con la del modelo del cliente
// (src/client/Models) para que el JSON viaje de ida y vuelta sin transformaciones.
// Las propiedades calculadas (get-only) NO se replican aquí: el cliente las recalcula
// al deserializar, y EF Core no debe mapearlas.

/// <summary>Institución de salud = tenant (aislamiento lógico RSK-03).</summary>
public class Institucion
{
    public int Id { get; set; }
    public string NIT { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ResolucionSanitaria { get; set; } = string.Empty;
    public PlanSaaS Plan { get; set; } = PlanSaaS.Profesional;
    public EstadoTenant Estado { get; set; } = EstadoTenant.Activo;
    public DateTime FechaRegistro { get; set; }
    public int TotalCamas { get; set; }
}

/// <summary>Usuario del sistema. Autenticación BCrypt + JWT (OBJ-02).</summary>
public class Usuario
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public RolSistema Rol { get; set; }
    public bool Activo { get; set; } = true;
    public bool PrimerIngreso { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    /// <summary>Nunca viaja en JSON: solo se usa para verificar el login (BCrypt).</summary>
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;
}

/// <summary>Hoja de vida del equipo biomédico (OBJ-03).</summary>
public class Equipo
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Responsable { get; set; } = string.Empty;
    public EstadoEquipo Estado { get; set; } = EstadoEquipo.Operativo;
    public DateTime FechaInstalacion { get; set; }
    public DateTime? UltimoMantenimiento { get; set; }
    public DateTime? ProximoMantenimiento { get; set; }
    public DateTime? UltimaCalibracion { get; set; }
    public DateTime? ProximaCalibracion { get; set; }
    public int VidaUtilMeses { get; set; } = 84;
    public decimal ValorAdquisicion { get; set; }
    public string Fabricante { get; set; } = string.Empty;
    public string ClaseRiesgo { get; set; } = "II";
    public string ResolucionAplicable { get; set; } = "Res. 3100/2019";

    /// <summary>Campos personalizados por tenant; se guarda como columna jsonb.</summary>
    public Dictionary<string, string> CamposDinamicos { get; set; } = new();

    public int OrdenesAbiertas { get; set; }
    public int TotalIntervenciones { get; set; }
}

/// <summary>Orden de trabajo con código único SIT-2026-000123.</summary>
public class OrdenTrabajo
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int EquipoId { get; set; }
    public string EquipoNombre { get; set; } = string.Empty;
    public string EquipoUbicacion { get; set; } = string.Empty;
    public TipoMantenimiento Tipo { get; set; } = TipoMantenimiento.Correctivo;
    public Prioridad Prioridad { get; set; } = Prioridad.Media;
    public EstadoOrden Estado { get; set; } = EstadoOrden.Nueva;

    public int SolicitanteId { get; set; }
    public string SolicitanteNombre { get; set; } = string.Empty;
    public int? TecnicoId { get; set; }
    public string TecnicoNombre { get; set; } = string.Empty;

    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaCierre { get; set; }
    public int SLAHoras { get; set; } = 24;
    public decimal CostoEstimado { get; set; }
    public decimal CostoReal { get; set; }
    public string ReporteSala { get; set; } = string.Empty;

    /// <summary>Línea de tiempo de trazabilidad (jsonb).</summary>
    public List<EventoOrden> Historial { get; set; } = new();

    /// <summary>Comentarios de seguimiento (jsonb).</summary>
    public List<ComentarioOrden> Comentarios { get; set; } = new();
}

/// <summary>Entrada de la línea de tiempo de la orden.</summary>
public class EventoOrden
{
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
}

/// <summary>Comentario / nota de seguimiento en la orden.</summary>
public class ComentarioOrden
{
    public DateTime Fecha { get; set; }
    public string Autor { get; set; } = string.Empty;
    public string RolAutor { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
}

/// <summary>Perfil técnico extendido del usuario con rol Técnico.</summary>
public class Tecnico
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public List<EspecialidadTecnica> Especialidades { get; set; } = new();
    public DisponibilidadTecnico Disponibilidad { get; set; } = DisponibilidadTecnico.Disponible;
    public double CapacidadHorasSemana { get; set; } = 40;
    public double HorasAsignadas { get; set; }
    public double HorasSemanaAnterior { get; set; }
    public int OrdenesCompletadas { get; set; }
    public int OrdenesActivas { get; set; }
    public double Calificacion { get; set; } = 4.5;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Turno { get; set; } = "Diurno";
}

/// <summary>Intervención técnica en campo (OBJ-05). En fase 3 migrará a MongoDB.</summary>
public class Intervencion
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int OrdenId { get; set; }
    public string OrdenCodigo { get; set; } = string.Empty;
    public int EquipoId { get; set; }
    public string EquipoNombre { get; set; } = string.Empty;
    public int TecnicoId { get; set; }
    public string TecnicoNombre { get; set; } = string.Empty;

    public EstadoIntervencion Estado { get; set; } = EstadoIntervencion.Borrador;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    public string Diagnostico { get; set; } = string.Empty;
    public string CausaRaiz { get; set; } = string.Empty;
    public string AccionesRealizadas { get; set; } = string.Empty;
    public string PruebasRealizadas { get; set; } = string.Empty;
    public string ResultadoPrueba { get; set; } = string.Empty;
    public string NotasCampo { get; set; } = string.Empty;
    public string Recomendaciones { get; set; } = string.Empty;

    public double HorasInvertidas { get; set; }
    public decimal CostoRepuestos { get; set; }
    public decimal ManoObra { get; set; }

    public List<RepuestoConsumido> Repuestos { get; set; } = new();
    public List<string> Evidencias { get; set; } = new();
}

public class RepuestoConsumido
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
}

/// <summary>Formulario con estructura dinámica definida por el tenant (OBJ-06).</summary>
public class FormularioDinamico
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public string Autor { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public int Aplicaciones { get; set; }
    public List<CampoDinamico> Campos { get; set; } = new();
}

public class CampoDinamico
{
    public int Id { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public TipoCampoDinamico Tipo { get; set; }
    public bool Requerido { get; set; }
    public string Ayuda { get; set; } = string.Empty;
    public List<string> Opciones { get; set; } = new();
    public int Orden { get; set; }
}

/// <summary>Bitácora de auditoría inmutable (RSK-01): solo INSERT, nunca UPDATE ni DELETE.</summary>
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
}

/// <summary>Notificación operativa del centro de avisos.</summary>
public class Notificacion
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Leida { get; set; }
    public string Tipo { get; set; } = "info";
    public string Icono { get; set; } = "";
}
