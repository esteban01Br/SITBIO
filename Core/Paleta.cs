using System.Windows.Media;

namespace SITBIO_Desktop.Core;

/// <summary>
/// Pinceles y colores de acento compartidos por todos los módulos, para que
/// estados, chips y tarjetas mantengan exactamente el mismo tono.
/// </summary>
public static class Paleta
{
    private static readonly Lazy<Brush> _exito = new(() => Pincel(ExitoHex));
    private static readonly Lazy<Brush> _exitoSuave = new(() => Pincel(ExitoSuaveHex));
    private static readonly Lazy<Brush> _advertencia = new(() => Pincel(AdvertenciaHex));
    private static readonly Lazy<Brush> _advertenciaSuave = new(() => Pincel(AdvertenciaSuaveHex));
    private static readonly Lazy<Brush> _peligro = new(() => Pincel(PeligroHex));
    private static readonly Lazy<Brush> _peligroSuave = new(() => Pincel(PeligroSuaveHex));
    private static readonly Lazy<Brush> _info = new(() => Pincel(InfoHex));
    private static readonly Lazy<Brush> _infoSuave = new(() => Pincel(InfoSuaveHex));
    private static readonly Lazy<Brush> _acento = new(() => Pincel(AcentoHex));
    private static readonly Lazy<Brush> _acentoSuave = new(() => Pincel(AcentoSuaveHex));
    private static readonly Lazy<Brush> _neutro = new(() => Pincel(NeutroHex));
    private static readonly Lazy<Brush> _neutroSuave = new(() => Pincel(NeutroSuaveHex));

    public const string ExitoHex = "#12924E";
    public const string ExitoSuaveHex = "#E6F6EE";
    public const string AdvertenciaHex = "#D97706";
    public const string AdvertenciaSuaveHex = "#FDF1E1";
    public const string PeligroHex = "#D13C4B";
    public const string PeligroSuaveHex = "#FCEBEC";
    public const string InfoHex = "#0B6FA4";
    public const string InfoSuaveHex = "#E8F2F9";
    public const string AcentoHex = "#12B5A6";
    public const string AcentoSuaveHex = "#E3F7F5";
    public const string NeutroHex = "#4E6076";
    public const string NeutroSuaveHex = "#EFF3F8";

    public static Brush Exito => _exito.Value;
    public static Brush ExitoSuave => _exitoSuave.Value;
    public static Brush Advertencia => _advertencia.Value;
    public static Brush AdvertenciaSuave => _advertenciaSuave.Value;
    public static Brush Peligro => _peligro.Value;
    public static Brush PeligroSuave => _peligroSuave.Value;
    public static Brush Info => _info.Value;
    public static Brush InfoSuave => _infoSuave.Value;
    public static Brush Acento => _acento.Value;
    public static Brush AcentoSuave => _acentoSuave.Value;
    public static Brush Neutro => _neutro.Value;
    public static Brush NeutroSuave => _neutroSuave.Value;

    public static Color ColorExito => ColorFromString(ExitoHex);
    public static Color ColorAdvertencia => ColorFromString(AdvertenciaHex);
    public static Color ColorPeligro => ColorFromString(PeligroHex);
    public static Color ColorInfo => ColorFromString(InfoHex);
    public static Color ColorAcento => ColorFromString(AcentoHex);
    public static Color ColorNeutro => ColorFromString(NeutroHex);

    /// <summary>Convierte "#RRGGBB" en pincel; devuelve neutro si el texto no es válido.</summary>
    public static Brush Pincel(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Neutro;
        try { return new SolidColorBrush(ColorFromString(hex)); }
        catch { return Neutro; }
    }

    public static Color ColorFromString(string hex)
    {
        try
        {
            if (ColorConverter.ConvertFromString(hex) is Color c) return c;
        }
        catch { /* color inválido: se usa el neutro */ }
        return Color.FromRgb(0x4E, 0x60, 0x76);
    }

    /// <summary>Devuelve el par (color sólido, fondo suave) de un nivel de severidad.</summary>
    public static (Brush Color, Brush Fondo) Severidad(string nivel) => nivel.ToLowerInvariant() switch
    {
        "exito" or "success" or "ok" => (Exito, ExitoSuave),
        "warning" or "advertencia" => (Advertencia, AdvertenciaSuave),
        "danger" or "peligro" or "error" => (Peligro, PeligroSuave),
        "acento" => (Acento, AcentoSuave),
        "info" => (Info, InfoSuave),
        _ => (Neutro, NeutroSuave)
    };
}
