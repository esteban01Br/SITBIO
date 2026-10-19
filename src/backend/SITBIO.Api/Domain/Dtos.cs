namespace SITBIO.Api.Domain;

// Contratos de la API (login e indicadores). Los nombres de propiedades deben ser
// idénticos a los del cliente WPF para que System.Text.Json los deserialice tal cual.

/// <summary>Petición de inicio de sesión.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;

    /// <summary>Opcional: si se informa, el usuario debe pertenecer a ese tenant.</summary>
    public int TenantId { get; set; }
}

/// <summary>Respuesta del login: token JWT + perfil del usuario.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public Usuario Usuario { get; set; } = new();
}

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

    public PuntoSerie(string etiqueta, double valor)
    {
        Etiqueta = etiqueta;
        Valor = valor;
    }

    public PuntoSerie(string etiqueta, decimal valor)
    {
        Etiqueta = etiqueta;
        ValorDecimal = valor;
        Valor = (double)valor;
    }
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
        Etiqueta = etiqueta;
        Valor = valor;
        Color = color;
    }
}
