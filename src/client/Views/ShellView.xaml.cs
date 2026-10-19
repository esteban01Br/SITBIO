using System.Windows;
using System.Windows.Controls;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// El selector de institución está enlazado en un solo sentido, así que el
    /// cambio hecho por el usuario se propaga por aquí. Se ignora cuando el
    /// tenant seleccionado ya es el activo para evitar ciclos al iniciar.
    /// </summary>
    private void Tenant_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ShellViewModel vm) return;
        if (sender is not ComboBox combo) return;
        if (combo.SelectedItem is not Institucion tenant) return;
        if (vm.Institucion?.Id == tenant.Id) return;

        vm.CambiarInstitucionCommand.Execute(tenant);
    }
}
