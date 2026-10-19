using System.Windows.Controls;

namespace SITBIO_Desktop.Views;

/// <summary>
/// Ficha de intervención técnica. Sin lógica: todo el comportamiento vive en
/// <see cref="ViewModels.IntervencionViewModel"/> (patrón MVVM).
/// </summary>
public partial class IntervencionView : UserControl
{
    public IntervencionView() => InitializeComponent();
}
