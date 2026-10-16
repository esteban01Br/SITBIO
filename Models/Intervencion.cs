using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Intervención técnica = enriquecimiento técnico progresivo en campo (OBJ-05).
/// El técnico va registrando diagnóstico, repuestos, pruebas, tiempos y notas.
/// </summary>
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

    public decimal CostoTotal => CostoRepuestos + ManoObra;

    public string EstadoTexto => Estado switch
    {
        EstadoIntervencion.Borrador => "Borrador",
        EstadoIntervencion.EnEjecucion => "En ejecución",
        EstadoIntervencion.Finalizada => "Finalizada",
        EstadoIntervencion.Validada => "Validada",
        _ => Estado.ToString()
    };

    public double AvanceCampos
    {
        get
        {
            var campos = new[]
            {
                !string.IsNullOrWhiteSpace(Diagnostico),
                !string.IsNullOrWhiteSpace(CausaRaiz),
                !string.IsNullOrWhiteSpace(AccionesRealizadas),
                !string.IsNullOrWhiteSpace(PruebasRealizadas),
                !string.IsNullOrWhiteSpace(ResultadoPrueba),
                HorasInvertidas > 0,
                Repuestos.Count > 0,
                !string.IsNullOrWhiteSpace(Recomendaciones)
            };
            return campos.Count(c => c) / (double)campos.Length * 100;
        }
    }
}

public class RepuestoConsumido
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
