using System.Windows;
using SITBIO_Desktop.Core;

namespace SITBIO_Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModels.RootViewModel();
    }
}
