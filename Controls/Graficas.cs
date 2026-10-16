using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Controls;

/// <summary>
/// Base de las gráficas del sistema. Se construyen en C# para tener control
/// exacto de la geometría y evitar dependencias externas.
/// </summary>
public abstract class GraficaBase : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(GraficaBase),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnItemsChanged));

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((GraficaBase)d).Renderizar();

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected static readonly Brush PincelRejilla = new SolidColorBrush(Color.FromRgb(0xED, 0xF1, 0xF6));
    protected static readonly Brush PincelTextoSuave = new SolidColorBrush(Color.FromRgb(0x84, 0x96, 0xAB));

    protected List<object> Elementos() => ItemsSource?.Cast<object>().ToList() ?? new List<object>();

    protected abstract void Renderizar();

    protected static double Doble(object elemento, string propiedad)
    {
        var tipo = elemento.GetType();
        var info = tipo.GetProperty(propiedad);
        var valor = info?.GetValue(elemento);
        return valor switch
        {
            double d => d,
            decimal m => (double)m,
            int i => i,
            _ => 0
        };
    }

    protected static string Texto(object elemento, string propiedad)
    {
        var info = elemento.GetType().GetProperty(propiedad);
        return info?.GetValue(elemento)?.ToString() ?? string.Empty;
    }

    protected static Brush PincelDesde(string hex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(0x0B, 0x6F, 0xA4));
        }
    }

    protected static TextBlock Etiqueta(double valor, string formato, Brush color, double tamano, FontWeight peso)
        => new()
        {
            Text = valor.ToString(formato, CultureInfo.InvariantCulture),
            FontSize = tamano,
            FontWeight = peso,
            Foreground = color,
            HorizontalAlignment = HorizontalAlignment.Center
        };
}

// ============================================================
//  BARRAS
// ============================================================
public class GraficaBarras : GraficaBase
{
    public static readonly DependencyProperty ColorBarraProperty =
        DependencyProperty.Register(nameof(ColorBarra), typeof(Brush), typeof(GraficaBarras),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x0B, 0x6F, 0xA4)), (d, _) => ((GraficaBarras)d).Renderizar()));

    public static readonly DependencyProperty AlturaProperty =
        DependencyProperty.Register(nameof(Altura), typeof(double), typeof(GraficaBarras),
            new PropertyMetadata(168d, (d, _) => ((GraficaBarras)d).Renderizar()));

    public static readonly DependencyProperty AnchoBarraProperty =
        DependencyProperty.Register(nameof(AnchoBarra), typeof(double), typeof(GraficaBarras),
            new PropertyMetadata(30d, (d, _) => ((GraficaBarras)d).Renderizar()));

    public static readonly DependencyProperty MostrarValoresProperty =
        DependencyProperty.Register(nameof(MostrarValores), typeof(bool), typeof(GraficaBarras),
            new PropertyMetadata(true, (d, _) => ((GraficaBarras)d).Renderizar()));

    public static readonly DependencyProperty FormatoProperty =
        DependencyProperty.Register(nameof(Formato), typeof(string), typeof(GraficaBarras),
            new PropertyMetadata("0.#", (d, _) => ((GraficaBarras)d).Renderizar()));

    public Brush ColorBarra { get => (Brush)GetValue(ColorBarraProperty); set => SetValue(ColorBarraProperty, value); }
    public double Altura { get => (double)GetValue(AlturaProperty); set => SetValue(AlturaProperty, value); }
    public double AnchoBarra { get => (double)GetValue(AnchoBarraProperty); set => SetValue(AnchoBarraProperty, value); }
    public bool MostrarValores { get => (bool)GetValue(MostrarValoresProperty); set => SetValue(MostrarValoresProperty, value); }
    public string Formato { get => (string)GetValue(FormatoProperty); set => SetValue(FormatoProperty, value); }

    protected override void Renderizar()
    {
        var datos = Elementos();
        if (datos.Count == 0)
        {
            Content = TextoVacio("Sin información para graficar");
            return;
        }

        var maximo = datos.Max(d => Doble(d, "Valor"));
        if (maximo <= 0) maximo = 1;

        var raiz = new Grid();
        raiz.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Altura) });
        raiz.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // ---- Área de trazado ----
        var trazado = new Grid();

        // Rejilla horizontal
        for (var i = 0; i <= 4; i++)
        {
            trazado.Children.Add(new Border
            {
                Height = 1,
                Background = PincelRejilla,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, Altura * i / 4d, 0, 0)
            });
        }

        var columnas = new Grid();
        foreach (var _ in datos) columnas.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < datos.Count; i++)
        {
            var item = datos[i];
            var valor = Doble(item, "Valor");
            var etiqueta = Texto(item, "Etiqueta");
            var alturaBarra = Math.Max(3d, valor / maximo * (Altura - (MostrarValores ? 20 : 0)));

            var columna = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(3, 0, 3, 0)
            };

            if (MostrarValores)
            {
                columna.Children.Add(Etiqueta(valor, Formato, new SolidColorBrush(Color.FromRgb(0x0E, 0x1B, 0x2C)),
                    11.5, FontWeights.SemiBold));
                columna.Children.Add(new Border { Height = 5 });
            }

            var barra = new Border
            {
                Width = AnchoBarra,
                Height = alturaBarra,
                CornerRadius = new CornerRadius(7, 7, 3, 3),
                Background = ColorBarra,
                HorizontalAlignment = HorizontalAlignment.Center,
                ToolTip = string.IsNullOrWhiteSpace(etiqueta) ? $"{valor.ToString(Formato, CultureInfo.InvariantCulture)}"
                                                              : $"{etiqueta}: {valor.ToString(Formato, CultureInfo.InvariantCulture)}",
                Cursor = Cursors.Arrow,
                Opacity = 0.94
            };
            columna.Children.Add(barra);

            Grid.SetColumn(columna, i);
            columnas.Children.Add(columna);
        }

        trazado.Children.Add(columnas);
        Grid.SetRow(trazado, 0);
        raiz.Children.Add(trazado);

        // ---- Etiquetas del eje ----
        var etiquetasEje = new Grid();
        foreach (var _ in datos) etiquetasEje.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < datos.Count; i++)
        {
            var texto = new TextBlock
            {
                Text = Texto(datos[i], "Etiqueta"),
                FontSize = 11,
                Foreground = PincelTextoSuave,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(3, 8, 3, 0)
            };
            Grid.SetColumn(texto, i);
            etiquetasEje.Children.Add(texto);
        }

        Grid.SetRow(etiquetasEje, 1);
        raiz.Children.Add(etiquetasEje);

        Content = raiz;
    }

    internal static TextBlock TextoVacio(string mensaje) => new()
    {
        Text = mensaje,
        FontSize = 12,
        Foreground = PincelTextoSuave,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        TextWrapping = TextWrapping.Wrap,
        TextAlignment = TextAlignment.Center,
        Margin = new Thickness(12)
    };
}

// ============================================================
//  LÍNEA / ÁREA
// ============================================================
public class GraficaLinea : GraficaBase
{
    private double _anchoRenderizado;

    public static readonly DependencyProperty ColorLineaProperty =
        DependencyProperty.Register(nameof(ColorLinea), typeof(Brush), typeof(GraficaLinea),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x12, 0xB5, 0xA6)), (d, _) => ((GraficaLinea)d).Renderizar()));

    public static readonly DependencyProperty AlturaProperty =
        DependencyProperty.Register(nameof(Altura), typeof(double), typeof(GraficaLinea),
            new PropertyMetadata(150d, (d, _) => ((GraficaLinea)d).Renderizar()));

    public static readonly DependencyProperty MostrarPuntosProperty =
        DependencyProperty.Register(nameof(MostrarPuntos), typeof(bool), typeof(GraficaLinea),
            new PropertyMetadata(true, (d, _) => ((GraficaLinea)d).Renderizar()));

    public static readonly DependencyProperty FormatoProperty =
        DependencyProperty.Register(nameof(Formato), typeof(string), typeof(GraficaLinea),
        new PropertyMetadata("0.#", (d, _) => ((GraficaLinea)d).Renderizar()));

    public Brush ColorLinea { get => (Brush)GetValue(ColorLineaProperty); set => SetValue(ColorLineaProperty, value); }
    public double Altura { get => (double)GetValue(AlturaProperty); set => SetValue(AlturaProperty, value); }
    public bool MostrarPuntos { get => (bool)GetValue(MostrarPuntosProperty); set => SetValue(MostrarPuntosProperty, value); }
    public string Formato { get => (string)GetValue(FormatoProperty); set => SetValue(FormatoProperty, value); }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (Math.Abs(sizeInfo.NewSize.Width - _anchoRenderizado) > 1.5)
            Renderizar();
    }

    protected override void Renderizar()
    {
        var datos = Elementos();
        _anchoRenderizado = ActualWidth;

        if (datos.Count == 0)
        {
            Content = GraficaBarras.TextoVacio("Sin información para graficar");
            return;
        }

        var ancho = Math.Max(ActualWidth, 260);
        var alto = Altura;
        var topReservado = 18d;

        var maximo = datos.Max(d => Doble(d, "Valor"));
        var minimo = datos.Min(d => Doble(d, "Valor"));
        if (maximo - minimo < 0.0001) maximo = minimo + 1;

        double X(int i) => datos.Count == 1 ? ancho / 2 : 14 + i * (ancho - 28) / (datos.Count - 1);
        double Y(double v) => topReservado + (1 - (v - minimo) / (maximo - minimo)) * (alto - topReservado - 16);

        var lienzo = new Canvas { Width = ancho, Height = alto };

        // Rejilla
        for (var i = 0; i <= 3; i++)
        {
            var linea = new Border
            {
                Height = 1,
                Width = ancho,
                Background = PincelRejilla
            };
            Canvas.SetLeft(linea, 0);
            Canvas.SetTop(linea, topReservado + i * (alto - topReservado - 16) / 3d);
            lienzo.Children.Add(linea);
        }

        var puntos = datos.Select((d, i) => new Point(X(i), Y(Doble(d, "Valor")))).ToList();

        // Área bajo la curva
        if (puntos.Count > 1)
        {
            var colorLinea = ColorLinea is SolidColorBrush solida ? solida.Color : Color.FromRgb(0x12, 0xB5, 0xA6);
            var area = new Polygon
            {
                Points = new PointCollection(puntos.Append(new Point(puntos[^1].X, alto)).Prepend(new Point(puntos[0].X, alto))),
                Fill = new SolidColorBrush(Color.FromArgb(34, colorLinea.R, colorLinea.G, colorLinea.B))
            };
            lienzo.Children.Add(area);

            var polilinea = new Polyline
            {
                Points = new PointCollection(puntos),
                Stroke = ColorLinea,
                StrokeThickness = 2.6,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            lienzo.Children.Add(polilinea);
        }

        // Puntos y etiquetas
        for (var i = 0; i < datos.Count; i++)
        {
            var valor = Doble(datos[i], "Valor");
            var punto = puntos[i];

            if (MostrarPuntos)
            {
                lienzo.Children.Add(new System.Windows.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    Fill = Brushes.White,
                    Stroke = ColorLinea,
                    StrokeThickness = 2.4,
                    ToolTip = $"{Texto(datos[i], "Etiqueta")}: {valor.ToString(Formato, CultureInfo.InvariantCulture)}"
                });
                Canvas.SetLeft(lienzo.Children[^1], punto.X - 4.5);
                Canvas.SetTop(lienzo.Children[^1], punto.Y - 4.5);
            }

            var textoValor = new TextBlock
            {
                Text = valor.ToString(Formato, CultureInfo.InvariantCulture),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x0E, 0x1B, 0x2C)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            textoValor.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(textoValor, punto.X - textoValor.DesiredSize.Width / 2);
            Canvas.SetTop(textoValor, Math.Max(0, punto.Y - 22));
            lienzo.Children.Add(textoValor);

            var textoEje = new TextBlock
            {
                Text = Texto(datos[i], "Etiqueta"),
                FontSize = 11,
                Foreground = PincelTextoSuave,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            textoEje.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(textoEje, Math.Max(0, Math.Min(ancho - textoEje.DesiredSize.Width, punto.X - textoEje.DesiredSize.Width / 2)));
            Canvas.SetTop(textoEje, alto - 15);
            lienzo.Children.Add(textoEje);
        }

        Content = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = lienzo
        };
    }
}

// ============================================================
//  DONA
// ============================================================
public class GraficaDona : GraficaBase
{
    public static readonly DependencyProperty TamanioProperty =
        DependencyProperty.Register(nameof(Tamanio), typeof(double), typeof(GraficaDona),
            new PropertyMetadata(178d, (d, _) => ((GraficaDona)d).Renderizar()));

    public static readonly DependencyProperty TextoCentroProperty =
        DependencyProperty.Register(nameof(TextoCentro), typeof(string), typeof(GraficaDona),
            new PropertyMetadata(string.Empty, (d, _) => ((GraficaDona)d).Renderizar()));

    public static readonly DependencyProperty SubtextoCentroProperty =
        DependencyProperty.Register(nameof(SubtextoCentro), typeof(string), typeof(GraficaDona),
            new PropertyMetadata(string.Empty, (d, _) => ((GraficaDona)d).Renderizar()));

    public static readonly DependencyProperty MostrarLeyendaProperty =
        DependencyProperty.Register(nameof(MostrarLeyenda), typeof(bool), typeof(GraficaDona),
            new PropertyMetadata(true, (d, _) => ((GraficaDona)d).Renderizar()));

    public double Tamanio { get => (double)GetValue(TamanioProperty); set => SetValue(TamanioProperty, value); }
    public string TextoCentro { get => (string)GetValue(TextoCentroProperty); set => SetValue(TextoCentroProperty, value); }
    public string SubtextoCentro { get => (string)GetValue(SubtextoCentroProperty); set => SetValue(SubtextoCentroProperty, value); }
    public bool MostrarLeyenda { get => (bool)GetValue(MostrarLeyendaProperty); set => SetValue(MostrarLeyendaProperty, value); }

    protected override void Renderizar()
    {
        var datos = Elementos();
        if (datos.Count == 0 || datos.Sum(d => Doble(d, "Valor")) <= 0)
        {
            Content = GraficaBarras.TextoVacio("Sin datos para distribuir");
            return;
        }

        var total = datos.Sum(d => Doble(d, "Valor"));
        var tam = Tamanio;
        var radio = tam / 2 - 2;
        var grosor = tam * 0.26;
        var centro = tam / 2;

        var raiz = new StackPanel { Orientation = Orientation.Horizontal };

        // ---- Rodillo ----
        var contenedor = new Grid { Width = tam, Height = tam, VerticalAlignment = VerticalAlignment.Center };
        var angulo = -90d;

        foreach (var segmento in datos)
        {
            var proporcion = Doble(segmento, "Valor") / total;
            var inicio = angulo;
            var fin = angulo + proporcion * 360;
            angulo = fin;

            var pincel = PincelDesde(Texto(segmento, "Color"));

            if (proporcion >= 0.9999)
            {
                contenedor.Children.Add(new System.Windows.Shapes.Ellipse
                {
                    Width = tam,
                    Height = tam,
                    Fill = pincel,
                    ToolTip = $"{Texto(segmento, "Etiqueta")}: {Doble(segmento, "Valor"):#.#}"
                });
            }
            else
            {
                var figura = new Path
                {
                    Fill = pincel,
                    ToolTip = $"{Texto(segmento, "Etiqueta")}: {Doble(segmento, "Valor"):#.#}"
                };
                figura.Data = Sector(centro, centro, radio, inicio, fin);
                contenedor.Children.Add(figura);
            }
        }

        // Hueco central (dona)
        contenedor.Children.Add(new System.Windows.Shapes.Ellipse
        {
            Width = radio * 2 - grosor * 2,
            Height = radio * 2 - grosor * 2,
            Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        });

        // Texto central
        if (!string.IsNullOrWhiteSpace(TextoCentro) || !string.IsNullOrWhiteSpace(SubtextoCentro))
        {
            var centroTexto = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (!string.IsNullOrWhiteSpace(TextoCentro))
                centroTexto.Children.Add(new TextBlock
                {
                    Text = TextoCentro,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x0E, 0x1B, 0x2C)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            if (!string.IsNullOrWhiteSpace(SubtextoCentro))
                centroTexto.Children.Add(new TextBlock
                {
                    Text = SubtextoCentro,
                    FontSize = 11,
                    Foreground = PincelTextoSuave,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 1, 0, 0)
                });
            contenedor.Children.Add(centroTexto);
        }

        raiz.Children.Add(contenedor);

        // ---- Leyenda ----
        if (MostrarLeyenda)
        {
            var leyenda = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(22, 0, 0, 0)
            };

            foreach (var segmento in datos)
            {
                var valor = Doble(segmento, "Valor");
                var porcentaje = valor / total * 100;
                var fila = new Grid { Margin = new Thickness(0, 0, 0, 11) };
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var punto = new Border
                {
                    Width = 9,
                    Height = 9,
                    CornerRadius = new CornerRadius(3),
                    Background = PincelDesde(Texto(segmento, "Color")),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(punto, 0);
                fila.Children.Add(punto);

                var nombre = new TextBlock
                {
                    Text = Texto(segmento, "Etiqueta"),
                    FontSize = 12.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x4E, 0x60, 0x76)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(9, 0, 8, 0),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 170
                };
                Grid.SetColumn(nombre, 1);
                fila.Children.Add(nombre);

                var cifra = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                cifra.Children.Add(new TextBlock
                {
                    Text = porcentaje.ToString("0.#", CultureInfo.InvariantCulture),
                    FontSize = 12.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x0E, 0x1B, 0x2C))
                });
                cifra.Children.Add(new TextBlock
                {
                    Text = " %",
                    FontSize = 10.5,
                    Foreground = PincelTextoSuave,
                    Margin = new Thickness(1, 3, 0, 0)
                });
                Grid.SetColumn(cifra, 2);
                fila.Children.Add(cifra);

                leyenda.Children.Add(fila);
            }

            raiz.Children.Add(leyenda);
        }

        Content = raiz;
    }

    private static Geometry Sector(double cx, double cy, double radio, double gradosInicio, double gradosFin)
    {
        // Normaliza para evitar arcos mayores de 180° de forma errónea
        var figura = new StreamGeometry();
        using (var contexto = figura.Open())
        {
            var inicio = Radianes(gradosInicio);
            var fin = Radianes(gradosFin);
            var p0 = new Point(cx + radio * Math.Cos(inicio), cy + radio * Math.Sin(inicio));
            var p1 = new Point(cx + radio * Math.Cos(fin), cy + radio * Math.Sin(fin));
            var arcoGrados = gradosFin - gradosInicio;
            var grande = arcoGrados > 180;

            contexto.BeginFigure(new Point(cx, cy), true, true);
            contexto.LineTo(p0, false, false);
            contexto.ArcTo(p1, new Size(radio, radio), 0, grande, SweepDirection.Clockwise, true, false);
            contexto.LineTo(new Point(cx, cy), false, false);
        }
        figura.Freeze();
        return figura;
    }

    private static double Radianes(double grados) => grados * Math.PI / 180d;
}
