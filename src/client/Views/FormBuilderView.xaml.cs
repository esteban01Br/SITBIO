using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

/// <summary>
/// Vista del diseñador de formularios dinámicos (FormBuilder).
/// La lógica vive en <see cref="FormBuilderViewModel"/>; aquí sólo se
/// refresca la vista previa cuando cambian propiedades de los modelos
/// (que no implementan INotifyPropertyChanged) y se filtra la entrada
/// de los campos numéricos.
/// </summary>
public partial class FormBuilderView : UserControl
{
    public FormBuilderView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Al cambiar el tipo de campo se fuerza la reevaluación de los
    /// disparadores (DataTrigger) de la vista previa, ya que los modelos
    /// no notifican cambios de propiedad.
    /// </summary>
    private void TipoCampo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefrescarVistaPrevia();
    }

    /// <summary>
    /// Igual que con el tipo: marcar/desmarcar «requerido» debe
    /// refrescar de inmediato el asterisco de la vista previa.
    /// </summary>
    private void RequeridoCampo_Cambiado(object sender, RoutedEventArgs e)
    {
        RefrescarVistaPrevia();
    }

    /// <summary>
    /// Sólo se permiten dígitos, coma decimal y signo menos en los
    /// campos numéricos de la vista previa.
    /// </summary>
    private void CampoNumerico_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !EsEntradaNumerica(e.Text);
    }

    private static bool EsEntradaNumerica(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return false;
        }

        foreach (char c in texto)
        {
            if (!char.IsDigit(c) && c != ',' && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    private void RefrescarVistaPrevia()
    {
        if (VistaPrevia is null || DataContext is not FormBuilderViewModel vm)
        {
            return;
        }

        // Desconectar y volver a enlazar la colección obliga a WPF a
        // recrear los elementos y reevaluar los DataTrigger del template.
        VistaPrevia.ItemsSource = null;
        VistaPrevia.ItemsSource = vm.Campos;
    }
}
