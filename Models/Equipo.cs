using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Hoja de vida del equipo biomédico (OBJ-03). Incluye campos dinámicos por
/// tenant (OBJ-06) almacenados en <see cref="CamposDinamicos"/>.
/// </summary>
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

    /// <summary>Campos personalizados definidos por el administrador del tenant.</summary>
    public Dictionary<string, string> CamposDinamicos { get; set; } = new();

    public int OrdenesAbiertas { get; set; }
    public int TotalIntervenciones { get; set; }

    public string EstadoTexto => Estado switch
    {
        EstadoEquipo.Operativo => "Operativo",
        EstadoEquipo.EnMantenimiento => "En mantenimiento",
        EstadoEquipo.EnCalibracion => "En calibración",
        EstadoEquipo.FueraDeServicio => "Fuera de servicio",
        EstadoEquipo.Baja => "Dado de baja",
        _ => Estado.ToString()
    };

    public bool PreventivoVencido => ProximoMantenimiento.HasValue && ProximoMantenimiento.Value < DateTime.Today;

    public double PorcentajeVida
    {
        get
        {
            if (VidaUtilMeses <= 0) return 0;
            var transcurrido = (DateTime.Today - FechaInstalacion).TotalDays / 30.44;
            return Math.Clamp(transcurrido / VidaUtilMeses * 100, 0, 100);
        }
    }

    public override string ToString() => $"{CodigoInventario} — {Nombre}";
}
