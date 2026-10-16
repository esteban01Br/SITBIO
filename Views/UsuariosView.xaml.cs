using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SITBIO_Desktop.Models;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

/// <summary>Padrón de usuarios: tabla, filtros y diálogo de alta.</summary>
public partial class UsuariosView : UserControl
{
    public UsuariosView() => InitializeComponent();

    /// <summary>Doble clic sobre una fila: abre la ficha del usuario.</summary>
    private void Usuarios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not UsuariosViewModel vm) return;
        if (sender is not DataGrid tabla) return;
        if (tabla.SelectedItem is Usuario usuario)
            vm.VerCommand.Execute(usuario);
    }
}
