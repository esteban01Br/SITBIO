using System.Windows.Controls;
using System.Windows.Input;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

public partial class ReportesView : UserControl
{
    public ReportesView() => InitializeComponent();

    /// <summary>Doble clic sobre una de las órdenes recientes: abre su detalle.</summary>
    private void OrdenesRecientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not ReportesViewModel vm) return;
        if (sender is not DataGrid tabla) return;
        if (tabla.SelectedItem is OrdenTrabajo orden)
            vm.VerOrdenCommand.Execute(orden);
    }
}
