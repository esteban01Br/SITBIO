using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Core;

/// <summary>Convierte un enum de estado en el color de su chip.</summary>
public class EstadoEquipoBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is EstadoEquipo e ? e switch
    {
        EstadoEquipo.Operativo => new SolidColorBrush(Color.FromRgb(0x12, 0x92, 0x4E)),
        EstadoEquipo.EnMantenimiento => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
        EstadoEquipo.EnCalibracion => new SolidColorBrush(Color.FromRgb(0x2E, 0x9B, 0xE5)),
        EstadoEquipo.FueraDeServicio => new SolidColorBrush(Color.FromRgb(0xD1, 0x3C, 0x4B)),
        EstadoEquipo.Baja => new SolidColorBrush(Color.FromRgb(0x84, 0x96, 0xAB)),
        _ => new SolidColorBrush(Color.FromRgb(0x4E, 0x60, 0x76))
    } : Brushes.Gray;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class EstadoOrdenBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is EstadoOrden e ? e switch
    {
        EstadoOrden.Nueva => new SolidColorBrush(Color.FromRgb(0x2E, 0x9B, 0xE5)),
        EstadoOrden.Asignada => new SolidColorBrush(Color.FromRgb(0x7A, 0x5A, 0xF8)),
        EstadoOrden.EnProceso => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
        EstadoOrden.EnEsperaRepuesto => new SolidColorBrush(Color.FromRgb(0xD1, 0x3C, 0x4B)),
        EstadoOrden.Verificacion => new SolidColorBrush(Color.FromRgb(0x12, 0xB5, 0xA6)),
        EstadoOrden.Cerrada => new SolidColorBrush(Color.FromRgb(0x12, 0x92, 0x4E)),
        EstadoOrden.Cancelada => new SolidColorBrush(Color.FromRgb(0x84, 0x96, 0xAB)),
        _ => new SolidColorBrush(Color.FromRgb(0x4E, 0x60, 0x76))
    } : Brushes.Gray;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class EstadoOrdenFondo : IValueConverter
{
    private readonly EstadoOrdenBrush _inner = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var color = ((SolidColorBrush)_inner.Convert(value, t, p, c)).Color;
        var claro = Color.FromRgb(
            (byte)Math.Min(255, color.R + 210),
            (byte)Math.Min(255, color.G + 210),
            (byte)Math.Min(255, color.B + 210));
        return new SolidColorBrush(claro);
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class EstadoEquipoFondo : IValueConverter
{
    private readonly EstadoEquipoBrush _inner = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var color = ((SolidColorBrush)_inner.Convert(value, t, p, c)).Color;
        return new SolidColorBrush(Color.FromRgb(
            (byte)Math.Min(255, color.R + 210),
            (byte)Math.Min(255, color.G + 210),
            (byte)Math.Min(255, color.B + 210)));
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class PrioridadBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is Prioridad e ? e switch
    {
        Prioridad.Critica => new SolidColorBrush(Color.FromRgb(0xD1, 0x3C, 0x4B)),
        Prioridad.Alta => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
        Prioridad.Media => new SolidColorBrush(Color.FromRgb(0x0B, 0x6F, 0xA4)),
        Prioridad.Baja => new SolidColorBrush(Color.FromRgb(0x84, 0x96, 0xAB)),
        _ => new SolidColorBrush(Color.FromRgb(0x4E, 0x60, 0x76))
    } : Brushes.Gray;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class PrioridadFondo : IValueConverter
{
    private readonly PrioridadBrush _inner = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var color = ((SolidColorBrush)_inner.Convert(value, t, p, c)).Color;
        return new SolidColorBrush(Color.FromRgb(
            (byte)Math.Min(255, color.R + 210),
            (byte)Math.Min(255, color.G + 210),
            (byte)Math.Min(255, color.B + 210)));
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class TipoMantenimientoBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is TipoMantenimiento e ? e switch
    {
        TipoMantenimiento.Preventivo => new SolidColorBrush(Color.FromRgb(0x0B, 0x6F, 0xA4)),
        TipoMantenimiento.Correctivo => new SolidColorBrush(Color.FromRgb(0xD1, 0x3C, 0x4B)),
        TipoMantenimiento.Predictivo => new SolidColorBrush(Color.FromRgb(0x7A, 0x5A, 0xF8)),
        TipoMantenimiento.Calibracion => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
        TipoMantenimiento.Instalacion => new SolidColorBrush(Color.FromRgb(0x12, 0xB5, 0xA6)),
        _ => new SolidColorBrush(Color.FromRgb(0x4E, 0x60, 0x76))
    } : Brushes.Gray;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Color de barra de carga: verde &lt;70 %, ámbar &lt;90 %, rojo arriba.</summary>
public class CargaBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var v = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        return v switch
        {
            < 70 => new SolidColorBrush(Color.FromRgb(0x12, 0x92, 0x4E)),
            < 90 => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
            _ => new SolidColorBrush(Color.FromRgb(0xD1, 0x3C, 0x4B))
        };
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Cumplimiento de SLA: verde ≥90, ámbar ≥75, rojo debajo.</summary>
public class SlaBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var v = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        return v switch
        {
            >= 90 => new SolidColorBrush(Color.FromRgb(0x12, 0x92, 0x4E)),
            >= 75 => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
            _ => new SolidColorBrush(Color.FromRgb(0xD1, 0x3C, 0x4B))
        };
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Texto vacío o nulo → Collapsed.</summary>
public class TextoVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) =>
        string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Nulo o vacío → Visible (útil para estados por defecto).</summary>
public class NuloVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) =>
        value is null || (value is string s && string.IsNullOrWhiteSpace(s)) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Invierte un booleano.</summary>
public class InversoBool : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is not true;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => value is not true;
}

/// <summary>Booleano → Visibility (evita declarar BooleanToVisibilityConverter en cada vista).</summary>
public class BoolVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Booleano invertido → Visibility (falso ⇒ Visible).</summary>
public class NoBoolVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Número mayor que cero → Visible (alertas, contadores).</summary>
public class CeroVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var numero = System.Convert.ToDouble(value ?? 0, CultureInfo.InvariantCulture);
        return numero > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Cadena "#RRGGBB" → pincel.</summary>
public class HexBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => Paleta.Pincel(value as string);

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Cadena "#RRGGBB" → color sólido.</summary>
public class HexColor : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => Paleta.ColorFromString(value as string ?? "#4E6076");

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Colección vacía → Collapsed (para estados de tabla vacía).</summary>
public class ColeccionVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) =>
        value is System.Collections.ICollection c2 && c2.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class ColeccionVaciaVisible : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) =>
        value is System.Collections.ICollection c3 && c3.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Nivel de acceso → color de la barra de permiso.</summary>
public class NivelAccesoBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is NivelAcceso n ? n switch
    {
        NivelAcceso.Total => new SolidColorBrush(Color.FromRgb(0x12, 0x92, 0x4E)),
        NivelAcceso.Lectura => new SolidColorBrush(Color.FromRgb(0x2E, 0x9B, 0xE5)),
        _ => new SolidColorBrush(Color.FromRgb(0x84, 0x96, 0xAB))
    } : Brushes.Gray;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

public class NivelAccesoTexto : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value is NivelAcceso n ? n switch
    {
        NivelAcceso.Total => "Lectura y escritura",
        NivelAcceso.Lectura => "Solo lectura",
        _ => "Sin acceso"
    } : "Sin acceso";

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Formato compacto de moneda colombiana.</summary>
public class MonedaFormato : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is decimal d) return Formato(d);
        if (value is double dd) return Formato((decimal)dd);
        if (value is int i) return Formato(i);
        return "$0";
    }

    private static string Formato(decimal d) => d >= 1_000_000_000
        ? $"${d / 1_000_000_000:N1} MM"
        : d >= 1_000_000
            ? $"${d / 1_000_000:N1} M"
            : d >= 1_000
                ? $"${d / 1_000:N0} mil"
                : $"${d:N0}";

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>Relativo ("hace 5 min") para fechas recientes.</summary>
public class FechaRelativa : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not DateTime fecha) return string.Empty;
        var delta = DateTime.Now - fecha;
        if (delta.TotalMinutes < 1) return "ahora mismo";
        if (delta.TotalMinutes < 60) return $"hace {(int)delta.TotalMinutes} min";
        if (delta.TotalHours < 24) return $"hace {(int)delta.TotalHours} h";
        if (delta.TotalDays < 30) return $"hace {(int)delta.TotalDays} d";
        return fecha.ToString("dd MMM yyyy");
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}
