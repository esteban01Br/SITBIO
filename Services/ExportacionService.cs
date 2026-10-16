using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SITBIO_Desktop.Core;
using SITBIO_Desktop.Models;

namespace SITBIO_Desktop.Services;

/// <summary>
/// Generación de reportes operacionales en Excel y PDF (OBJ-06 y §4.3 del
/// DOC-01-FCP: indicadores exportables listos para auditoría INVIMA / Secretaría
/// de Salud conforme a la Resolución 3100 de 2019).
/// </summary>
public interface IExportacionService
{
    string ExportarIndicadoresExcel(int tenantId);
    string ExportarOrdenesExcel(int tenantId, IEnumerable<OrdenTrabajo> ordenes);
    string ExportarInventarioExcel(int tenantId);
    string ExportarIndicadoresPdf(int tenantId);
    string ExportarOrdenesPdf(int tenantId, IEnumerable<OrdenTrabajo> ordenes);
}

public class ExportacionService : IExportacionService
{
    private readonly IRepositorioDatos _datos;
    private readonly ISesionService _sesion;
    private readonly IAuditoriaService _auditoria;
    private readonly IToastService _toast;

    static ExportacionService()
    {
        // Licencia comunitaria de QuestPDF (uso académico / sin ánimo de lucro).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ExportacionService(IRepositorioDatos datos, ISesionService sesion,
        IAuditoriaService auditoria, IToastService toast)
    {
        _datos = datos;
        _sesion = sesion;
        _auditoria = auditoria;
        _toast = toast;
    }

    private static string CarpetaReportes()
    {
        var carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SITBIO", "Reportes");
        Directory.CreateDirectory(carpeta);
        return carpeta;
    }

    private static string NuevaRuta(string prefijo, string extension)
        => Path.Combine(CarpetaReportes(),
            $"{prefijo}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}");

    private void RegistrarExportacion(string formato, string archivo)
    {
        _auditoria.Registrar(AccionAuditoria.Exportacion, "Reporte",
            Path.GetFileName(archivo), $"Exportación a {formato}");
        _toast.Exito($"Archivo guardado en Mis documentos\\SITBIO\\Reportes", "Exportación completada");
    }

    // =========================== EXCEL ===========================

    private void CabeceraExcel(IXLWorksheet hoja, string titulo, int columnas)
    {
        hoja.Cell(1, 1).Value = "SITBIO — Gestión de Mantenimiento de Equipos Biomédicos";
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 14;
        hoja.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#0B6FA4");

        hoja.Cell(2, 1).Value = titulo;
        hoja.Cell(2, 1).Style.Font.FontSize = 12;
        hoja.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#4E6076");

        var tenant = _sesion.TenantActual;
        hoja.Cell(3, 1).Value = $"Institución: {tenant?.Nombre}  ·  NIT {tenant?.NIT}  ·  Generado {DateTime.Now:dd/MM/yyyy HH:mm}";
        hoja.Cell(3, 1).Style.Font.FontSize = 10;
        hoja.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml("#8496AB");

        hoja.Cell(4, 1).Value = "Res. 3100 de 2019 — Reporte para auditoría INVIMA / Secretaría de Salud";
        hoja.Cell(4, 1).Style.Font.FontSize = 9;
        hoja.Cell(4, 1).Style.Font.FontColor = XLColor.FromHtml("#8496AB");

        for (var c = 1; c <= columnas; c++)
        {
            var cabecera = hoja.Cell(6, c);
            cabecera.Style.Font.Bold = true;
            cabecera.Style.Font.FontColor = XLColor.White;
            cabecera.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B6FA4");
            cabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            cabecera.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cabecera.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cabecera.Style.Border.OutsideBorderColor = XLColor.FromHtml("#08567F");
        }

        hoja.Cell(5, 1).Value = "";
        hoja.Row(6).Height = 22;
        hoja.SheetView.FreezeRows(6);
    }

    private void AjustarColumnas(IXLWorksheet hoja, int columnas, int filas)
    {
        for (var c = 1; c <= columnas; c++)
            hoja.Column(c).AdjustToContents(1, Math.Max(filas + 6, 10), 8, 60);
    }

    public string ExportarIndicadoresExcel(int tenantId)
    {
        var ind = _datos.Indicadores(tenantId);
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Indicadores");

        CabeceraExcel(hoja, "Indicadores consolidados de disponibilidad y mantenimiento", 4);

        var filas = new (string Indicador, object Valor, string Unidad)[]
        {
            ("Equipos biomédicos registrados", ind.EquiposRegistrados, "unidades"),
            ("Equipos operativos", ind.EquiposOperativos, "unidades"),
            ("Disponibilidad operativa", ind.DisponibilidadPorcentaje, "%"),
            ("Órdenes abiertas", ind.OrdenesAbiertas, "órdenes"),
            ("Órdenes críticas / altas", ind.OrdenesCriticas, "órdenes"),
            ("Preventivos vencidos", ind.PreventivosVencidos, "equipos"),
            ("Calibraciones en 30 días", ind.CalibracionesProximas, "equipos"),
            ("Cumplimiento de SLA", ind.CumplimientoSLA, "%"),
            ("MTBF (tiempo medio entre fallas)", ind.MtbfHoras, "horas"),
            ("MTTR (tiempo medio de reparación)", ind.MttrHoras, "horas"),
            ("Costo de mantenimiento del mes", ind.CostoMes, "COP"),
        };

        var fila = 7;
        foreach (var (indicador, valor, unidad) in filas)
        {
            hoja.Cell(fila, 1).Value = indicador;
            hoja.Cell(fila, 2).Value = Convert.ToDouble(valor, CultureInfo.InvariantCulture);
            hoja.Cell(fila, 3).Value = unidad;
            if (unidad == "COP") hoja.Cell(fila, 2).Style.NumberFormat.Format = "$ #,##0";
            if (unidad == "%") hoja.Cell(fila, 2).Style.NumberFormat.Format = "0.0";
            if (fila % 2 == 0)
                for (var c = 1; c <= 3; c++)
                    hoja.Cell(fila, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F9FD");
            fila++;
        }

        AjustarColumnas(hoja, 4, filas.Length);
        var ruta = NuevaRuta("Indicadores_SITBIO", "xlsx");
        libro.SaveAs(ruta);
        RegistrarExportacion("Excel", ruta);
        return ruta;
    }

    public string ExportarOrdenesExcel(int tenantId, IEnumerable<OrdenTrabajo> ordenes)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Órdenes de trabajo");

        var lista = ordenes.ToList();
        CabeceraExcel(hoja, $"Órdenes de trabajo ({lista.Count} registros)", 13);

        var cabeceras = new[]
        {
            "Código de seguimiento", "Título", "Equipo", "Ubicación", "Tipo", "Prioridad",
            "Estado", "Solicitante", "Técnico asignado", "Fecha solicitud", "SLA (h)",
            "Costo real (COP)", "Fecha cierre"
        };
        for (var i = 0; i < cabeceras.Length; i++) hoja.Cell(6, i + 1).Value = cabeceras[i];

        var fila = 7;
        foreach (var o in lista)
        {
            hoja.Cell(fila, 1).Value = o.Codigo;
            hoja.Cell(fila, 1).Style.Font.Bold = true;
            hoja.Cell(fila, 2).Value = o.Titulo;
            hoja.Cell(fila, 3).Value = o.EquipoNombre;
            hoja.Cell(fila, 4).Value = o.EquipoUbicacion;
            hoja.Cell(fila, 5).Value = o.TipoTexto;
            hoja.Cell(fila, 6).Value = o.PrioridadTexto;
            hoja.Cell(fila, 7).Value = o.EstadoTexto;
            hoja.Cell(fila, 8).Value = o.SolicitanteNombre;
            hoja.Cell(fila, 9).Value = o.TecnicoNombre;
            hoja.Cell(fila, 10).Value = o.FechaSolicitud.ToString("dd/MM/yyyy HH:mm");
            hoja.Cell(fila, 11).Value = o.SLAHoras;
            hoja.Cell(fila, 12).Value = (double)o.CostoReal;
            hoja.Cell(fila, 12).Style.NumberFormat.Format = "$ #,##0";
            hoja.Cell(fila, 13).Value = o.FechaCierre?.ToString("dd/MM/yyyy HH:mm") ?? "—";

            if (o.Estado == EstadoOrden.Cerrada)
                for (var c = 1; c <= 13; c++)
                    hoja.Cell(fila, c).Style.Font.FontColor = XLColor.FromHtml("#12924E");
            else if (o.SLAVencido)
                for (var c = 1; c <= 13; c++)
                    hoja.Cell(fila, c).Style.Font.FontColor = XLColor.FromHtml("#D13C4B");

            fila++;
        }

        AjustarColumnas(hoja, 13, lista.Count);
        var ruta = NuevaRuta("Ordenes_SITBIO", "xlsx");
        libro.SaveAs(ruta);
        RegistrarExportacion("Excel", ruta);
        return ruta;
    }

    public string ExportarInventarioExcel(int tenantId)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Inventario biomédico");

        var equipos = _datos.EquiposDe(tenantId).OrderBy(e => e.CodigoInventario).ToList();
        CabeceraExcel(hoja, $"Hoja de vida del parque biomédico ({equipos.Count} activos)", 13);

        var cabeceras = new[]
        {
            "Código", "Equipo", "Categoría", "Marca", "Modelo", "N° serie", "Ubicación",
            "Estado", "Fecha instalación", "Último mantenimiento", "Próximo mantenimiento",
            "Clase de riesgo", "Valor de adquisición (COP)"
        };
        for (var i = 0; i < cabeceras.Length; i++) hoja.Cell(6, i + 1).Value = cabeceras[i];

        var fila = 7;
        foreach (var e in equipos)
        {
            hoja.Cell(fila, 1).Value = e.CodigoInventario;
            hoja.Cell(fila, 1).Style.Font.Bold = true;
            hoja.Cell(fila, 2).Value = e.Nombre;
            hoja.Cell(fila, 3).Value = e.Categoria;
            hoja.Cell(fila, 4).Value = e.Marca;
            hoja.Cell(fila, 5).Value = e.Modelo;
            hoja.Cell(fila, 6).Value = e.NumeroSerie;
            hoja.Cell(fila, 7).Value = e.Ubicacion;
            hoja.Cell(fila, 8).Value = e.EstadoTexto;
            hoja.Cell(fila, 9).Value = e.FechaInstalacion.ToString("dd/MM/yyyy");
            hoja.Cell(fila, 10).Value = e.UltimoMantenimiento?.ToString("dd/MM/yyyy") ?? "—";
            hoja.Cell(fila, 11).Value = e.ProximoMantenimiento?.ToString("dd/MM/yyyy") ?? "—";
            hoja.Cell(fila, 12).Value = e.ClaseRiesgo;
            hoja.Cell(fila, 13).Value = (double)e.ValorAdquisicion;
            hoja.Cell(fila, 13).Style.NumberFormat.Format = "$ #,##0";

            if (e.PreventivoVencido)
                hoja.Cell(fila, 11).Style.Font.FontColor = XLColor.FromHtml("#D13C4B");
            if (e.Estado == EstadoEquipo.FueraDeServicio || e.Estado == EstadoEquipo.Baja)
                hoja.Cell(fila, 8).Style.Font.FontColor = XLColor.FromHtml("#D13C4B");

            fila++;
        }

        AjustarColumnas(hoja, 13, equipos.Count);
        var ruta = NuevaRuta("Inventario_SITBIO", "xlsx");
        libro.SaveAs(ruta);
        RegistrarExportacion("Excel", ruta);
        return ruta;
    }

    // ============================ PDF ============================

    /// <summary>Encabezado común de los informes PDF de SITBIO.</summary>
    private static void EncabezadoInforme(PageDescriptor page, string subtitulo, Institucion? tenant)
    {
        page.Header().Column(col =>
        {
            col.Item().Text("SITBIO — Gestión de Mantenimiento de Equipos Biomédicos")
                .FontSize(15).FontColor("#0B6FA4").Bold();
            col.Item().Text(subtitulo).FontSize(11).FontColor("#4E6076");
            col.Item().PaddingTop(3).LineHorizontal(1).LineColor("#E2E9F1");
            col.Item().PaddingTop(4).Text(txt =>
            {
                txt.Span("Institución: ").SemiBold();
                txt.Span(tenant?.Nombre ?? "—");
                txt.Span("   ·   NIT ").SemiBold();
                txt.Span(tenant?.NIT ?? "—");
                txt.Span("   ·   Generado ").SemiBold();
                txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            });
        });
    }

    public string ExportarIndicadoresPdf(int tenantId)
    {
        var ind = _datos.Indicadores(tenantId);
        var tenant = _sesion.TenantActual;

        var documento = Document.Create(contenedor => contenedor.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(34);
            page.DefaultTextStyle(t => t.FontFamily("Segoe UI").FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().Text("SITBIO — Indicadores de Disponibilidad y Mantenimiento")
                    .FontSize(15).FontColor("#0B6FA4").Bold();
                col.Item().PaddingTop(3).LineHorizontal(1).LineColor("#E2E9F1");
                col.Item().PaddingTop(4).Text(txt =>
                {
                    txt.Span("Institución: ").SemiBold();
                    txt.Span(tenant?.Nombre ?? "—");
                    txt.Span("   ·   Generado ").SemiBold();
                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });

            page.Content().PaddingVertical(14).Column(col =>
            {
                col.Item().Text("Resumen operativo").FontSize(12).Bold().FontColor("#0E1B2C");
                col.Item().PaddingTop(8).Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.ConstantColumn(120);
                        c.RelativeColumn();
                        c.ConstantColumn(120);
                    });

                    void Celda(string etiqueta, string valor, string color)
                    {
                        tabla.Cell().Background("#F8FAFD").Padding(8).Border(0.5f).BorderColor("#E2E9F1")
                            .Text(etiqueta).FontColor("#4E6076");
                        tabla.Cell().Background("#F8FAFD").Padding(8).Border(0.5f).BorderColor("#E2E9F1")
                            .AlignRight().Text(valor).Bold().FontColor(color);
                    }

                    Celda("Equipos registrados", ind.EquiposRegistrados.ToString(), "#0E1B2C");
                    Celda("Disponibilidad operativa", $"{ind.DisponibilidadPorcentaje:0.0} %", "#12924E");
                    Celda("Órdenes abiertas", ind.OrdenesAbiertas.ToString(), "#0E1B2C");
                    Celda("Cumplimiento de SLA", $"{ind.CumplimientoSLA:0.0} %", "#12924E");
                    Celda("Preventivos vencidos", ind.PreventivosVencidos.ToString(),
                        ind.PreventivosVencidos > 0 ? "#D97706" : "#12924E");
                    Celda("Calibraciones próximas", ind.CalibracionesProximas.ToString(), "#0E1B2C");
                    Celda("MTBF", $"{ind.MtbfHoras:0.0} h", "#0B6FA4");
                    Celda("MTTR", $"{ind.MttrHoras:0.0} h", "#0B6FA4");
                    Celda("Órdenes críticas / altas", ind.OrdenesCriticas.ToString(), "#D13C4B");
                    Celda("Costo del mes", ind.CostoMes.ToString("$ #,##0"), "#0E1B2C");
                });

                col.Item().PaddingTop(18).Text("Distribución de órdenes por estado").FontSize(12).Bold();
                col.Item().PaddingTop(6).Table(tabla =>
                {
                    tabla.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(90); c.RelativeColumn(); });
                    foreach (var s in _datos.DistribucionPorEstado(tenantId))
                    {
                        tabla.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#EDF1F6").Text(s.Etiqueta);
                        tabla.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#EDF1F6").AlignRight().Text(s.Valor.ToString()).Bold();
                        tabla.Cell().Padding(5).BorderBottom(0.5f).BorderColor("#EDF1F6")
                            .Width(14).Height(8).Background(s.Color);
                    }
                });
            });

            page.Footer().AlignCenter().Text(txt =>
            {
                txt.Span("Res. 3100 de 2019 — Reporte para auditoría INVIMA / Secretaría de Salud   ·   Página ")
                    .FontColor("#8496AB").FontSize(8);
                txt.CurrentPageNumber().FontColor("#8496AB").FontSize(8);
                txt.Span(" de ").FontColor("#8496AB").FontSize(8);
                txt.TotalPages().FontColor("#8496AB").FontSize(8);
            });
        }));

        var ruta = NuevaRuta("Indicadores_SITBIO", "pdf");
        documento.GeneratePdf(ruta);
        RegistrarExportacion("PDF", ruta);
        return ruta;
    }

    public string ExportarOrdenesPdf(int tenantId, IEnumerable<OrdenTrabajo> ordenes)
    {
        var tenant = _sesion.TenantActual;
        var lista = ordenes.ToList();

        var doc = Document.Create(contenedor => contenedor.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(28);
            page.DefaultTextStyle(t => t.FontFamily("Segoe UI").FontSize(8.5f));

            page.Header().Column(col =>
            {
                col.Item().Text("SITBIO — Órdenes de trabajo")
                    .FontSize(15).FontColor("#0B6FA4").Bold();
                col.Item().Text($"Reporte de trazabilidad de mantenimientos ({lista.Count} registros)")
                    .FontSize(10).FontColor("#4E6076");
                col.Item().PaddingTop(3).LineHorizontal(1).LineColor("#E2E9F1");
                col.Item().PaddingTop(3).Text(txt =>
                {
                    txt.Span("Institución: ").SemiBold();
                    txt.Span(tenant?.Nombre ?? "—");
                    txt.Span("   ·   Generado ").SemiBold();
                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });

            page.Content().PaddingTop(12).Table(tabla =>
            {
                tabla.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(92);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                    c.ConstantColumn(84);
                    c.ConstantColumn(74);
                    c.ConstantColumn(74);
                });

                void Cabecera(string texto) => tabla.Cell().Background("#0B6FA4").Padding(6)
                    .Text(texto).FontColor("#FFFFFF").Bold().FontSize(8);

                Cabecera("Código");
                Cabecera("Título");
                Cabecera("Equipo / ubicación");
                Cabecera("Tipo");
                Cabecera("Prioridad");
                Cabecera("Estado");
                Cabecera("Técnico");
                Cabecera("Fecha solicitud");
                Cabecera("Costo");

                var alterno = false;
                foreach (var o in lista)
                {
                    var fondo = alterno ? "#F8FAFD" : "#FFFFFF";
                    alterno = !alterno;

                    void Celda(string texto, string? color = null)
                    {
                        var celda = tabla.Cell().Background(fondo).BorderBottom(0.4f).BorderColor("#E2E9F1").Padding(6);
                        if (color is null) celda.Text(texto);
                        else celda.Text(texto).FontColor(color);
                    }

                    tabla.Cell().Background(fondo).BorderBottom(0.4f).BorderColor("#E2E9F1").Padding(6)
                        .Text(o.Codigo).Bold().FontColor("#0B6FA4");
                    Celda(o.Titulo);
                    Celda($"{o.EquipoNombre}\n{o.EquipoUbicacion}");
                    Celda(o.TipoTexto);
                    Celda(o.PrioridadTexto, o.Prioridad == Prioridad.Critica ? "#D13C4B" : null);
                    Celda(o.EstadoTexto, o.Estado == EstadoOrden.Cerrada ? "#12924E" : null);
                    Celda(o.TecnicoNombre);
                    Celda(o.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"));
                    Celda(o.CostoReal > 0 ? o.CostoReal.ToString("$ #,##0") : "—");
                }
            });

            page.Footer().AlignCenter().Text(txt =>
            {
                txt.Span("Res. 3100 de 2019 — Documento trazable para auditoría   ·   Página ")
                    .FontColor("#8496AB").FontSize(7.5f);
                txt.CurrentPageNumber().FontColor("#8496AB").FontSize(7.5f);
                txt.Span(" de ").FontColor("#8496AB").FontSize(7.5f);
                txt.TotalPages().FontColor("#8496AB").FontSize(7.5f);
            });
        }));

        var ruta = NuevaRuta("Ordenes_SITBIO", "pdf");
        doc.GeneratePdf(ruta);
        RegistrarExportacion("PDF", ruta);
        return ruta;
    }
}
