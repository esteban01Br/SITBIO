using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView() => InitializeComponent();

    /// <summary>Doble clic sobre una orden de la tabla: abre su detalle.</summary>
    private void Ordenes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not DashboardViewModel vm) return;
        if (sender is not DataGrid tabla) return;
        if (tabla.SelectedItem is OrdenTrabajo orden)
            vm.VerOrdenCommand.Execute(orden);
    }

    /// <summary>Clic sobre un equipo de la lista: abre su hoja de vida.</summary>
    private void Equipo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not DashboardViewModel vm) return;
        if (sender is not FrameworkElement elemento) return;
        if (elemento.DataContext is Equipo equipo)
            vm.VerEquipoCommand.Execute(equipo);
    }
}
