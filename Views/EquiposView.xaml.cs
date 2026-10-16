using System.Windows.Controls;
using System.Windows.Input;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

public partial class EquiposView : UserControl
{
    public EquiposView() => InitializeComponent();

    /// <summary>Doble clic sobre un equipo de la tabla: abre su hoja de vida.</summary>
    private void Equipos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not EquiposViewModel vm) return;
        if (sender is not DataGrid tabla) return;
        if (tabla.SelectedItem is Equipo equipo)
            vm.VerCommand.Execute(equipo);
    }
}
