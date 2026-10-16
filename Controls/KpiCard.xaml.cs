using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SITBIO_Desktop.Controls;

/// <summary>
/// Tarjeta de indicador (KPI) usada en el panel de control y en reportes.
/// </summary>
public partial class KpiCard : UserControl
{
    public KpiCard() => InitializeComponent();

    public static readonly DependencyProperty TituloProperty =
        DependencyProperty.Register(nameof(Titulo), typeof(string), typeof(KpiCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValorProperty =
        DependencyProperty.Register(nameof(Valor), typeof(string), typeof(KpiCard), new PropertyMetadata("0"));

    public static readonly DependencyProperty UnidadProperty =
        DependencyProperty.Register(nameof(Unidad), typeof(string), typeof(KpiCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconoProperty =
        DependencyProperty.Register(nameof(Icono), typeof(string), typeof(KpiCard), new PropertyMetadata("\uE946"));

    public static readonly DependencyProperty IconoColorProperty =
        DependencyProperty.Register(nameof(IconoColor), typeof(Color), typeof(KpiCard),
            new PropertyMetadata(Color.FromRgb(0x0B, 0x6F, 0xA4)));

    public static readonly DependencyProperty IconoFondoProperty =
        DependencyProperty.Register(nameof(IconoFondo), typeof(Brush), typeof(KpiCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE8, 0xF2, 0xF9))));

    public static readonly DependencyProperty DeltaTextoProperty =
        DependencyProperty.Register(nameof(DeltaTexto), typeof(string), typeof(KpiCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DeltaIconoProperty =
        DependencyProperty.Register(nameof(DeltaIcono), typeof(string), typeof(KpiCard), new PropertyMetadata("\uE74A"));

    public static readonly DependencyProperty DeltaColorProperty =
        DependencyProperty.Register(nameof(DeltaColor), typeof(Brush), typeof(KpiCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x12, 0x92, 0x4E))));

    public static readonly DependencyProperty DeltaFondoProperty =
        DependencyProperty.Register(nameof(DeltaFondo), typeof(Brush), typeof(KpiCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE6, 0xF6, 0xEE))));

    public static readonly DependencyProperty LeyendaProperty =
        DependencyProperty.Register(nameof(Leyenda), typeof(string), typeof(KpiCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AyudaProperty =
        DependencyProperty.Register(nameof(Ayuda), typeof(string), typeof(KpiCard), new PropertyMetadata(string.Empty));

    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Valor { get => (string)GetValue(ValorProperty); set => SetValue(ValorProperty, value); }
    public string Unidad { get => (string)GetValue(UnidadProperty); set => SetValue(UnidadProperty, value); }
    public string Icono { get => (string)GetValue(IconoProperty); set => SetValue(IconoProperty, value); }
    public Color IconoColor { get => (Color)GetValue(IconoColorProperty); set => SetValue(IconoColorProperty, value); }
    public Brush IconoFondo { get => (Brush)GetValue(IconoFondoProperty); set => SetValue(IconoFondoProperty, value); }
    public string DeltaTexto { get => (string)GetValue(DeltaTextoProperty); set => SetValue(DeltaTextoProperty, value); }
    public string DeltaIcono { get => (string)GetValue(DeltaIconoProperty); set => SetValue(DeltaIconoProperty, value); }
    public Brush DeltaColor { get => (Brush)GetValue(DeltaColorProperty); set => SetValue(DeltaColorProperty, value); }
    public Brush DeltaFondo { get => (Brush)GetValue(DeltaFondoProperty); set => SetValue(DeltaFondoProperty, value); }
    public string Leyenda { get => (string)GetValue(LeyendaProperty); set => SetValue(LeyendaProperty, value); }
    public string Ayuda { get => (string)GetValue(AyudaProperty); set => SetValue(AyudaProperty, value); }
}
