using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SITBIO_Desktop.Controls;

/// <summary>
/// Estado vacío / sin resultados: mantiene un mismo criterio visual en todos
/// los módulos cuando no hay registros que mostrar.
/// </summary>
public partial class EmptyState : UserControl
{
    public EmptyState() => InitializeComponent();

    public static readonly DependencyProperty IconoProperty =
        DependencyProperty.Register(nameof(Icono), typeof(string), typeof(EmptyState), new PropertyMetadata(""));

    public static readonly DependencyProperty TituloProperty =
        DependencyProperty.Register(nameof(Titulo), typeof(string), typeof(EmptyState), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MensajeProperty =
        DependencyProperty.Register(nameof(Mensaje), typeof(string), typeof(EmptyState), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconoColorProperty =
        DependencyProperty.Register(nameof(IconoColor), typeof(Color), typeof(EmptyState),
            new PropertyMetadata(Color.FromRgb(0x93, 0xA5, 0xB8)));

    public static readonly DependencyProperty IconoFondoProperty =
        DependencyProperty.Register(nameof(IconoFondo), typeof(Brush), typeof(EmptyState),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xEF, 0xF3, 0xF8))));

    public static readonly DependencyProperty TextoAccionProperty =
        DependencyProperty.Register(nameof(TextoAccion), typeof(string), typeof(EmptyState), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AccionCommandProperty =
        DependencyProperty.Register(nameof(AccionCommand), typeof(ICommand), typeof(EmptyState), new PropertyMetadata(null));

    public static readonly DependencyProperty AccionParametroProperty =
        DependencyProperty.Register(nameof(AccionParametro), typeof(object), typeof(EmptyState), new PropertyMetadata(null));

    public string Icono { get => (string)GetValue(IconoProperty); set => SetValue(IconoProperty, value); }
    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Mensaje { get => (string)GetValue(MensajeProperty); set => SetValue(MensajeProperty, value); }
    public Color IconoColor { get => (Color)GetValue(IconoColorProperty); set => SetValue(IconoColorProperty, value); }
    public Brush IconoFondo { get => (Brush)GetValue(IconoFondoProperty); set => SetValue(IconoFondoProperty, value); }
    public string TextoAccion { get => (string)GetValue(TextoAccionProperty); set => SetValue(TextoAccionProperty, value); }
    public ICommand? AccionCommand { get => (ICommand?)GetValue(AccionCommandProperty); set => SetValue(AccionCommandProperty, value); }
    public object? AccionParametro { get => GetValue(AccionParametroProperty); set => SetValue(AccionParametroProperty, value); }
}
