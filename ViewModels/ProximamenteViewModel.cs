using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Services;

namespace SITBIO_Desktop.ViewModels;

/// <summary>Entregable prometido para una próxima fase del producto.</summary>
public class Entregable
{
    public string Icono { get; init; } = "\uE946";
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
}

/// <summary>
/// Pantalla informativa de módulos aún no construidos (calibraciones e
/// inventario): explica qué traerá la próxima fase y regresa al panel.
/// </summary>
public partial class ProximamenteViewModel : ObservableObject
{
    [ObservableProperty] private string _clave = string.Empty;
    [ObservableProperty] private string _titulo = "Módulo en construcción";
    [ObservableProperty] private string _subtitulo = "Próximamente en SITBIO";
    [ObservableProperty] private string _icono = "\uE946";
    [ObservableProperty] private string _mensaje = string.Empty;
    [ObservableProperty] private string _resumen = string.Empty;

    public ObservableCollection<Entregable> Coleccion { get; } = new();

    public ProximamenteViewModel(string clave)
    {
        Clave = string.IsNullOrWhiteSpace(clave) ? "calibraciones" : clave.Trim().ToLowerInvariant();

        if (Clave == "inventario") ConstruirInventario();
        else ConstruirCalibraciones();
    }

    /// <summary>Plan de calibraciones: trazabilidad metrológica del parque (OBJ-05).</summary>
    private void ConstruirCalibraciones()
    {
        Titulo = "Calibraciones";
        Subtitulo = "Trazabilidad metrológica · próxima fase";
        Icono = "\uE823";
        Mensaje = "El módulo de calibraciones aún está en construcción. Aquí se centralizará el ciclo completo de la trazabilidad metrológica del parque biomédico: desde la programación de la calibración hasta el certificado del patrón de referencia, con evidencia lista para el INVIMA.";
        Resumen = "Fase 2 · 4 entregables que cierran el objetivo OBJ-05 de control metrológico y cumplimiento normativo.";

        Coleccion.Clear();

        Coleccion.Add(new Entregable
        {
            Icono = "\uE787", // calendario
            Titulo = "Próximas calibraciones",
            Descripcion = "Calendario por equipo con recordatorios 30, 15 y 7 días antes de la fecha límite."
        });

        Coleccion.Add(new Entregable
        {
            Icono = "\uE72E", // escudo
            Titulo = "Vencimientos INVIMA",
            Descripcion = "Control de certificados y vencimientos ante el INVIMA y la Secretaría de Salud (Resolución 3100 de 2019)."
        });

        Coleccion.Add(new Entregable
        {
            Icono = "\uE7BA", // advertencia
            Titulo = "Alertas automáticas",
            Descripcion = "Notificación al coordinador biomédico cuando un equipo se queda sin margen de calibración."
        });

        Coleccion.Add(new Entregable
        {
            Icono = "\uE8A7", // detalle
            Titulo = "Historial de patrones",
            Descripcion = "Ruta completa de patrones, incertidumbres, resultados y firma del técnico responsable."
        });
    }

    /// <summary>Inventario: existencias del almacén biomédico ligadas a las órdenes.</summary>
    private void ConstruirInventario()
    {
        Titulo = "Inventario de repuestos";
        Subtitulo = "Almacén biomédico · próxima fase";
        Icono = "\uE81D";
        Mensaje = "El módulo de inventario aún está en construcción. Gestionará repuestos, consumibles y existencias del almacén biomédico, ligando cada consumo a la orden de trabajo que lo originó y avisando cuando un artículo llegue a su mínimo.";
        Resumen = "Fase 2 · 4 entregables que conectan el almacén con las órdenes de trabajo y el costo real de mantenimiento.";

        Coleccion.Clear();

        Coleccion.Add(new Entregable
        {
            Icono = "\uE912", // equipos
            Titulo = "Repuestos y consumibles",
            Descripcion = "Catálogo del almacén con código, proveedor, ubicación y existencia disponible."
        });

        Coleccion.Add(new Entregable
        {
            Icono = "\uE74B", // flecha abajo
            Titulo = "Mínimos y máximos",
            Descripcion = "Puntos de reorden por artículo, con alerta automática al llegar al stock mínimo."
        });

        Coleccion.Add(new Entregable
        {
            Icono = "\uE896", // descargar
            Titulo = "Órdenes de compra",
            Descripcion = "Solicitud de reposición con proveedor, costo estimado y estado de entrega."
        });

        Coleccion.Add(new Entregable
        {
            Icono = "\uE90F", // órdenes
            Titulo = "Consumos por orden",
            Descripcion = "Descuento de repuestos desde la intervención, con costo real por orden de trabajo."
        });
    }

    // --------------------------- Comandos ---------------------------

    [RelayCommand]
    private void Volver() => AppServicios.Navegacion.Navegar(Rutas.Dashboard);
}
