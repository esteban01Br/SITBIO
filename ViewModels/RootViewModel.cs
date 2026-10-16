using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Notificación flotante mostrada en la aplicación.</summary>
public partial class ToastViewModel : ObservableObject
{
    public string Titulo { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public string Icono { get; init; } = "\uE946";
    public string ColorClave { get; init; } = "Info";

    [ObservableProperty] private bool _visible = true;

    [RelayCommand]
    private void Cerrar() => Visible = false;
}

/// <summary>
/// ViewModel raíz: decide si la ventana muestra la pantalla de acceso o el
/// contenedor principal, y administra el panel de notificaciones.
/// </summary>
public partial class RootViewModel : ObservableObject
{
    private readonly ISesionService _sesion = AppServicios.Sesion;
    private readonly DispatcherTimer _relojToasts = new() { Interval = TimeSpan.FromMilliseconds(4500) };

    [ObservableProperty] private object? _pantalla;

    public ObservableCollection<ToastViewModel> Toasts { get; } = new();

    public RootViewModel()
    {
        _sesion.SesionCambiada += ActualizarPantalla;
        AppServicios.Toast.ToastSolicitado += RecibirToast;
        _relojToasts.Tick += (_, _) => DescartarToastsVencidos();
        _relojToasts.Start();

        Pantalla = new LoginViewModel(_sesion, AppServicios.Navegacion, AppServicios.Auditoria, AppServicios.Toast);
    }

    private void ActualizarPantalla()
    {
        Pantalla = _sesion.Autenticado
            ? new ShellViewModel(_sesion, AppServicios.Navegacion, AppServicios.Datos, AppServicios.Auditoria, AppServicios.Toast)
            : new LoginViewModel(_sesion, AppServicios.Navegacion, AppServicios.Auditoria, AppServicios.Toast);
    }

    private void RecibirToast(Toast toast)
    {
        var vm = new ToastViewModel
        {
            Titulo = toast.Titulo,
            Mensaje = toast.Mensaje,
            Icono = toast.Nivel switch
            {
                "exito" => "\uE73E",
                "warning" => "\uE7BA",
                "error" => "\uE783",
                _ => "\uE946"
            },
            ColorClave = toast.Nivel switch
            {
                "exito" => "Exito",
                "warning" => "Advertencia",
                "error" => "Peligro",
                _ => "Info"
            }
        };

        void Agregar() => Toasts.Add(vm);
        System.Windows.Application.Current?.Dispatcher.Invoke(Agregar);
    }

    private void DescartarToastsVencidos()
    {
        for (var i = Toasts.Count - 1; i >= 0; i--)
            if (!Toasts[i].Visible)
                Toasts.RemoveAt(i);

        if (Toasts.Count == 0) _relojToasts.Stop();
        else _relojToasts.Start();
    }
}
