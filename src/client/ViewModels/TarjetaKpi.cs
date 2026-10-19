using System.Windows.Media;
using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.ViewModels;

/// <summary>
/// Tarjeta de indicador reutilizable. Se construye en el ViewModel con
/// <see cref="Crear"/> para que todos los módulos muestren KPIs con el mismo
/// criterio visual.
/// </summary>
public class TarjetaKpi
{
    public string Titulo { get; init; } = string.Empty;
    public string Valor { get; init; } = "0";
    public string Unidad { get; init; } = string.Empty;
    public string Icono { get; init; } = "";
    public Color IconoColor { get; init; } = Paleta.ColorInfo;
    public Brush IconoFondo { get; init; } = Paleta.InfoSuave;

    public string Delta { get; init; } = string.Empty;
    public string DeltaIcono { get; init; } = "";
    public Brush DeltaColor { get; init; } = Paleta.Neutro;
    public Brush DeltaFondo { get; init; } = Paleta.NeutroSuave;

    public string Leyenda { get; init; } = string.Empty;
    public string Ayuda { get; init; } = string.Empty;

    /// <summary>
    /// Crea una tarjeta con severidad de color: exito, advertencia, peligro,
    /// info, acento o neutro.
    /// </summary>
    public static TarjetaKpi Crear(
        string titulo, string valor, string icono, string severidad,
        string delta = "", string leyenda = "", string ayuda = "",
        string? deltaIcono = null, string unidad = "")
    {
        var (color, fondo) = Paleta.Severidad(severidad);

        return new TarjetaKpi
        {
            Titulo = titulo,
            Valor = valor,
            Unidad = unidad,
            Icono = icono,
            IconoColor = color is SolidColorBrush pincel ? pincel.Color : Paleta.ColorInfo,
            IconoFondo = fondo,
            Delta = delta,
            DeltaIcono = deltaIcono ?? "",
            DeltaColor = color,
            DeltaFondo = fondo,
            Leyenda = leyenda,
            Ayuda = ayuda
        };
    }
}
