using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

/// <summary>
/// Bandeja de órdenes: tabla y tablero Kanban. El code-behind solo resuelve
/// la apertura de la orden desde una fila o desde una tarjeta del tablero.
/// </summary>
public partial class OrdenesView : UserControl
{
    public OrdenesView() => InitializeComponent();

    private OrdenesViewModel? Modelo => DataContext as OrdenesViewModel;

    private void Orden_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is OrdenTrabajo orden)
            Modelo?.VerCommand.Execute(orden);
    }

    private void Orden_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Evita abrir la orden cuando el clic fue sobre el botón de avanzar estado.
        if (DentroDeBoton(e.OriginalSource)) return;

        if (sender is FrameworkElement elemento && elemento.DataContext is OrdenTrabajo orden)
            Modelo?.VerCommand.Execute(orden);
    }

    private static bool DentroDeBoton(object? origen)
    {
        DependencyObject? actual = origen as DependencyObject;

        while (actual is not null)
        {
            if (actual is Button) return true;
            actual = VisualTreeHelper.GetParent(actual) ?? (actual as FrameworkElement)?.Parent as DependencyObject;
        }

        return false;
    }
}
