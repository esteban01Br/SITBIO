using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Institución de salud = tenant. El aislamiento lógico multi-tenant se garantiza
/// filtrando por <see cref="Id"/> en toda consulta (Patrón Row Level Security, RSK-03).
/// </summary>
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
    public int TotalEquipos => 0; // se calcula en la capa de servicios

    /// <summary>Iniciales para el avatar del tenant en la barra superior.</summary>
    public string Iniciales
    {
        get
        {
            var partes = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) return "??";
            if (partes.Length == 1) return partes[0][..2].ToUpperInvariant();
            return (partes[0][..1] + partes[1][..1]).ToUpperInvariant();
        }
    }

    public override string ToString() => Nombre;
}
