using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SITBIO_Desktop.Controls;

/// <summary>
/// Encabezado estándar de todos los módulos: icono, título, subtítulo,
/// contador de resultados y zona de acciones a la derecha.
/// </summary>
public partial class PageHeader : UserControl
{
    public PageHeader() => InitializeComponent();

    public static readonly DependencyProperty TituloProperty =
        DependencyProperty.Register(nameof(Titulo), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtituloProperty =
        DependencyProperty.Register(nameof(Subtitulo), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconoProperty =
        DependencyProperty.Register(nameof(Icono), typeof(string), typeof(PageHeader), new PropertyMetadata(""));

    public static readonly DependencyProperty IconoColorProperty =
        DependencyProperty.Register(nameof(IconoColor), typeof(Color), typeof(PageHeader),
            new PropertyMetadata(Color.FromRgb(0x0B, 0x6F, 0xA4)));

    public static readonly DependencyProperty IconoFondoProperty =
        DependencyProperty.Register(nameof(IconoFondo), typeof(Brush), typeof(PageHeader),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE8, 0xF2, 0xF9))));

    public static readonly DependencyProperty ContadorProperty =
        DependencyProperty.Register(nameof(Contador), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty NotaContadorProperty =
        DependencyProperty.Register(nameof(NotaContador), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AccionesProperty =
        DependencyProperty.Register(nameof(Acciones), typeof(object), typeof(PageHeader), new PropertyMetadata(null));

    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Subtitulo { get => (string)GetValue(SubtituloProperty); set => SetValue(SubtituloProperty, value); }
    public string Icono { get => (string)GetValue(IconoProperty); set => SetValue(IconoProperty, value); }
    public Color IconoColor { get => (Color)GetValue(IconoColorProperty); set => SetValue(IconoColorProperty, value); }
    public Brush IconoFondo { get => (Brush)GetValue(IconoFondoProperty); set => SetValue(IconoFondoProperty, value); }
    public string Contador { get => (string)GetValue(ContadorProperty); set => SetValue(ContadorProperty, value); }
    public string NotaContador { get => (string)GetValue(NotaContadorProperty); set => SetValue(NotaContadorProperty, value); }
    public object? Acciones { get => GetValue(AccionesProperty); set => SetValue(AccionesProperty, value); }
}
