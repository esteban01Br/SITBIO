using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Orden de trabajo. El campo <see cref="Codigo"/> es el código único de
/// seguimiento (radicado) que elimina la pérdida de trazabilidad descrita en §3.1.
/// </summary>
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

    public List<EventoOrden> Historial { get; set; } = new();
    public List<ComentarioOrden> Comentarios { get; set; } = new();

    public bool EstaAbierta => Estado is not (EstadoOrden.Cerrada or EstadoOrden.Cancelada);

    public DateTime FechaLimite => FechaSolicitud.AddHours(SLAHoras);

    /// <summary>Horas restantes (negativo = incumplimiento de SLA).</summary>
    public double HorasRestantes
    {
        get
        {
            var fin = FechaCierre ?? (EstaAbierta ? DateTime.Now : FechaSolicitud);
            return (FechaLimite - fin).TotalHours;
        }
    }

    public bool SLAVencido => EstaAbierta && HorasRestantes < 0;

    public double ProgresoSLA
    {
        get
        {
            if (SLAHoras <= 0) return 100;
            var usadas = (DateTime.Now - FechaSolicitud).TotalHours;
            return Math.Clamp(usadas / SLAHoras * 100, 0, 100);
        }
    }

    public ColumnaKanban Columna => Estado switch
    {
        EstadoOrden.Nueva or EstadoOrden.Asignada => ColumnaKanban.PorHacer,
        EstadoOrden.EnProceso => ColumnaKanban.EnCurso,
        EstadoOrden.EnEsperaRepuesto => ColumnaKanban.EnRevision,
        EstadoOrden.Verificacion => ColumnaKanban.EnPruebas,
        _ => ColumnaKanban.Completada
    };

    public string EstadoTexto => Estado switch
    {
        EstadoOrden.Nueva => "Nueva",
        EstadoOrden.Asignada => "Asignada",
        EstadoOrden.EnProceso => "En proceso",
        EstadoOrden.EnEsperaRepuesto => "Espera de repuesto",
        EstadoOrden.Verificacion => "Verificación",
        EstadoOrden.Cerrada => "Cerrada",
        EstadoOrden.Cancelada => "Cancelada",
        _ => Estado.ToString()
    };

    public string PrioridadTexto => Prioridad.ToString();

    public string TipoTexto => Tipo switch
    {
        TipoMantenimiento.Preventivo => "Preventivo",
        TipoMantenimiento.Correctivo => "Correctivo",
        TipoMantenimiento.Predictivo => "Predictivo",
        TipoMantenimiento.Calibracion => "Calibración",
        TipoMantenimiento.Instalacion => "Instalación",
        _ => Tipo.ToString()
    };

    public override string ToString() => Codigo;
}

/// <summary>Entrada de la línea de tiempo de la orden (trazabilidad).</summary>
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
