using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SITBIO_Desktop.Controls;

/// <summary>
/// Diálogo modal genérico reutilizable en todos los módulos (formularios,
/// confirmaciones y detalle de registros).
/// </summary>
public partial class Dialogo : UserControl
{
    public Dialogo() => InitializeComponent();

    public static readonly DependencyProperty VisibleProperty =
        DependencyProperty.Register(nameof(Visible), typeof(bool), typeof(Dialogo), new PropertyMetadata(false));

    public static readonly DependencyProperty TituloProperty =
        DependencyProperty.Register(nameof(Titulo), typeof(string), typeof(Dialogo), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtituloProperty =
        DependencyProperty.Register(nameof(Subtitulo), typeof(string), typeof(Dialogo), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ContenidoProperty =
        DependencyProperty.Register(nameof(Contenido), typeof(object), typeof(Dialogo), new PropertyMetadata(null));

    public static readonly DependencyProperty IconoProperty =
        DependencyProperty.Register(nameof(Icono), typeof(string), typeof(Dialogo), new PropertyMetadata(""));

    public static readonly DependencyProperty IconoColorProperty =
        DependencyProperty.Register(nameof(IconoColor), typeof(Color), typeof(Dialogo),
            new PropertyMetadata(Color.FromRgb(0x0B, 0x6F, 0xA4)));

    public static readonly DependencyProperty IconoFondoProperty =
        DependencyProperty.Register(nameof(IconoFondo), typeof(Brush), typeof(Dialogo),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE8, 0xF2, 0xF9))));

    public static readonly DependencyProperty TextoConfirmarProperty =
        DependencyProperty.Register(nameof(TextoConfirmar), typeof(string), typeof(Dialogo), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextoCancelarProperty =
        DependencyProperty.Register(nameof(TextoCancelar), typeof(string), typeof(Dialogo), new PropertyMetadata("Cancelar"));

    public static readonly DependencyProperty ConfirmarCommandProperty =
        DependencyProperty.Register(nameof(ConfirmarCommand), typeof(ICommand), typeof(Dialogo), new PropertyMetadata(null));

    public static readonly DependencyProperty CancelarCommandProperty =
        DependencyProperty.Register(nameof(CancelarCommand), typeof(ICommand), typeof(Dialogo), new PropertyMetadata(null));

    public static readonly DependencyProperty ParametroConfirmarProperty =
        DependencyProperty.Register(nameof(ParametroConfirmar), typeof(object), typeof(Dialogo), new PropertyMetadata(null));

    public static readonly DependencyProperty EsPeligroProperty =
        DependencyProperty.Register(nameof(EsPeligro), typeof(bool), typeof(Dialogo), new PropertyMetadata(false));

    public static readonly DependencyProperty AnchoProperty =
        DependencyProperty.Register(nameof(Ancho), typeof(double), typeof(Dialogo), new PropertyMetadata(560d));

    public bool Visible { get => (bool)GetValue(VisibleProperty); set => SetValue(VisibleProperty, value); }
    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Subtitulo { get => (string)GetValue(SubtituloProperty); set => SetValue(SubtituloProperty, value); }
    public object? Contenido { get => GetValue(ContenidoProperty); set => SetValue(ContenidoProperty, value); }
    public string Icono { get => (string)GetValue(IconoProperty); set => SetValue(IconoProperty, value); }
    public Color IconoColor { get => (Color)GetValue(IconoColorProperty); set => SetValue(IconoColorProperty, value); }
    public Brush IconoFondo { get => (Brush)GetValue(IconoFondoProperty); set => SetValue(IconoFondoProperty, value); }
    public string TextoConfirmar { get => (string)GetValue(TextoConfirmarProperty); set => SetValue(TextoConfirmarProperty, value); }
    public string TextoCancelar { get => (string)GetValue(TextoCancelarProperty); set => SetValue(TextoCancelarProperty, value); }
    public ICommand? ConfirmarCommand { get => (ICommand?)GetValue(ConfirmarCommandProperty); set => SetValue(ConfirmarCommandProperty, value); }
    public ICommand? CancelarCommand { get => (ICommand?)GetValue(CancelarCommandProperty); set => SetValue(CancelarCommandProperty, value); }
    public object? ParametroConfirmar { get => GetValue(ParametroConfirmarProperty); set => SetValue(ParametroConfirmarProperty, value); }
    public bool EsPeligro { get => (bool)GetValue(EsPeligroProperty); set => SetValue(EsPeligroProperty, value); }
    public double Ancho { get => (double)GetValue(AnchoProperty); set => SetValue(AnchoProperty, value); }
}
