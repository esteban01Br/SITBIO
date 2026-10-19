using System.Windows;
using System.Windows.Controls;
using SITBIO_Desktop.ViewModels;

namespace SITBIO_Desktop.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && !string.IsNullOrEmpty(vm.Contrasena))
            PassBox.Password = vm.Contrasena;
    }

    private void PassBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Contrasena = ((PasswordBox)sender).Password;
    }
}
