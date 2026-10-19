using System.Globalization;

namespace SITBIO_Desktop.Core;

/// <summary>
/// Formato numérico y monetario según convenciones locales (es-CO), usado por
/// todos los módulos para que cifras y porcentajes se lean igual en toda la app.
/// </summary>
public static class Formateo
{
    public static readonly CultureInfo Local = CultureInfo.GetCultureInfo("es-CO");

    public static string Num(double valor, string formato = "0.#") => valor.ToString(formato, Local);

    public static string Num(decimal valor, string formato = "0.#") => valor.ToString(formato, Local);

    public static string Entero(int valor) => valor.ToString("N0", Local);

    public static string Porcentaje(double valor) => valor.ToString("0.0", Local) + " %";

    public static string Dinero(decimal valor) => valor.ToString("$ #,##0", Local);

    public static string Fecha(DateTime fecha) => fecha.ToString("dd MMM yyyy", Local);

    public static string FechaHora(DateTime fecha) => fecha.ToString("dd MMM yyyy · HH:mm", Local);
}
