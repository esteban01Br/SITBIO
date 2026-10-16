namespace SITBIO_Desktop.Models;

/// <summary>Indicadores del panel principal (CU-03 / CU-05).</summary>
public class IndicadoresDashboard
{
    public int EquiposRegistrados { get; set; }
    public int EquiposOperativos { get; set; }
    public int OrdenesAbiertas { get; set; }
    public int OrdenesCriticas { get; set; }
    public int PreventivosVencidos { get; set; }
    public int CalibracionesProximas { get; set; }
    public double DisponibilidadPorcentaje { get; set; }
    public double CumplimientoSLA { get; set; }
    public double MtbfHoras { get; set; }
    public double MttrHoras { get; set; }
    public decimal CostoMes { get; set; }
    public decimal CostoMesAnterior { get; set; }

    public decimal VariacionCosto => CostoMesAnterior == 0 ? 0
        : (CostoMes - CostoMesAnterior) / CostoMesAnterior * 100;
}

/// <summary>Par clave/valor para las series de las gráficas.</summary>
public class PuntoSerie
{
    public string Etiqueta { get; set; } = string.Empty;
    public double Valor { get; set; }
    public decimal? ValorDecimal { get; set; }

    public PuntoSerie() { }
    public PuntoSerie(string etiqueta, double valor) { Etiqueta = etiqueta; Valor = valor; }
    public PuntoSerie(string etiqueta, decimal valor) { Etiqueta = etiqueta; ValorDecimal = valor; Valor = (double)valor; }
}

/// <summary>Distribución categórica (gráfica de dona / sectores).</summary>
public class SegmentoDistribucion
{
    public string Etiqueta { get; set; } = string.Empty;
    public double Valor { get; set; }
    public string Color { get; set; } = "#0B6FA4";

    public SegmentoDistribucion() { }
    public SegmentoDistribucion(string etiqueta, double valor, string color)
    {
        Etiqueta = etiqueta; Valor = valor; Color = color;
    }
}

/// <summary>Resumen de actividad reciente para la línea de tiempo del panel.</summary>
public class ActividadReciente
{
    public DateTime Fecha { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Icono { get; set; } = "\uE90A";
    public string Color { get; set; } = "#0B6FA4";
}
