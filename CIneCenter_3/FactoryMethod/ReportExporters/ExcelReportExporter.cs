using ASP_MVC_Prueba.Interfaces;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;

namespace ASP_MVC_Prueba.FactoryMethod.ReportExporters
{
    public class ExcelReportExporter : IReportExporter
    {
        public ExcelReportExporter()
        {
            // Configure EPPlus license for version 8+
            try
            {
                // For EPPlus version 8+, use the new License approach
                ExcelPackage.License.SetNonCommercialOrganization("CineCenter");
            }
            catch (Exception ex)
            {
                // If the new approach fails (older version), try the legacy approach
                try
                {
                    // For EPPlus versions 5-7
                    var licenseContextProperty = typeof(ExcelPackage).GetProperty("LicenseContext");
                    if (licenseContextProperty != null)
                    {
                        // Get the LicenseContext enum value for NonCommercial
                        var nonCommercialValue = Enum.Parse(licenseContextProperty.PropertyType, "NonCommercial");
                        licenseContextProperty.SetValue(null, nonCommercialValue);
                    }
                }
                catch
                {
                    // Log or handle the exception if both approaches fail
                    // This is a fallback and should only occur in rare cases
                }
            }
        }

        public async Task<FileResult> ExportReportePelisGanancia(ResumenReporteViewModel model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Peliculas_Ganancias_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            // No need to set license here anymore - it's now in the constructor
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Ganancias por Película");
                
                // Encabezados
                worksheet.Cells[1, 1].Value = "Título";
                worksheet.Cells[1, 2].Value = "Tickets Vendidos";
                worksheet.Cells[1, 3].Value = "Ingresos Totales ($)";
                
                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                // Datos
                int row = 2;
                foreach (var pelicula in model.DetallesPeliculas)
                {
                    worksheet.Cells[row, 1].Value = pelicula.Titulo;
                    worksheet.Cells[row, 2].Value = pelicula.TotalTicketsVendidos;
                    worksheet.Cells[row, 3].Value = pelicula.IngresosTotales;
                    
                    // Formato de moneda
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                    row++;
                }
                
                // Resumen
                row += 2;
                worksheet.Cells[row, 1].Value = "Resumen:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                
                row++;
                worksheet.Cells[row, 1].Value = "Total Películas:";
                worksheet.Cells[row, 2].Value = model.TotalPeliculas;
                
                row++;
                worksheet.Cells[row, 1].Value = "Total Tickets Vendidos:";
                worksheet.Cells[row, 2].Value = model.TotalTicketsGlobal;
                
                row++;
                worksheet.Cells[row, 1].Value = "Ingresos Totales:";
                worksheet.Cells[row, 2].Value = model.IngresosTotalesGlobal;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                
                // Ajustar ancho de columnas
                worksheet.Cells.AutoFitColumns();
                
                // Generar el archivo
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        public async Task<FileResult> ExportReporteGeneros(List<ReporteGeneroViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Generos_Populares_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            // Configure EPPlus license for version 8+
            try
            {
                // For EPPlus version 8+, use the new License approach
                ExcelPackage.License.SetNonCommercialOrganization("CineCenter");
            }
            catch
            {
                // For EPPlus versions 5-7
                try
                {
                    var licenseContextProperty = typeof(ExcelPackage).GetProperty("LicenseContext");
                    if (licenseContextProperty != null)
                    {
                        var nonCommercialValue = Enum.Parse(licenseContextProperty.PropertyType, "NonCommercial");
                        licenseContextProperty.SetValue(null, nonCommercialValue);
                    }
                }
                catch
                {
                    // Fallback if both approaches fail
                }
            }
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Géneros Populares");
                
                // Título del reporte
                worksheet.Cells[1, 1].Value = "REPORTE DE GÉNEROS MÁS POPULARES";
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Fecha del reporte
                worksheet.Cells[2, 1].Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                worksheet.Cells[2, 1, 2, 6].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Italic = true;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Datos de resumen
                var totalGeneros = model.Count;
                var totalTickets = model.Sum(g => g.TotalTickets);
                var totalIngresos = model.Sum(g => g.IngresosTotales);
                var totalPeliculas = model.Sum(g => g.CantidadPeliculas);
                
                int row = 4;
                
                // Resumen
                worksheet.Cells[row, 1].Value = "Total de Géneros:";
                worksheet.Cells[row, 2].Value = totalGeneros;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Total de Películas:";
                worksheet.Cells[row, 2].Value = totalPeliculas;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Total de Tickets Vendidos:";
                worksheet.Cells[row, 2].Value = totalTickets;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Ingresos Totales:";
                worksheet.Cells[row, 2].Value = totalIngresos;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row += 2;
                
                // Top 3 géneros
                worksheet.Cells[row, 1].Value = "Top 3 Géneros Más Populares:";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1, row, 3].Merge = true;
                row++;
                
                foreach (var genero in model.Take(3))
                {
                    worksheet.Cells[row, 1].Value = $"• {genero.Genero}";
                    worksheet.Cells[row, 2].Value = genero.PorcentajeVentas / 100; // Como decimal para formato porcentaje
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "0.0%";
                    worksheet.Cells[row, 3].Value = $"({genero.TotalTickets} tickets)";
                    row++;
                }
                row++;
                
                // Encabezados de tabla
                var headers = new[] { "#", "Género", "Películas", "Tickets Vendidos", "% del Total", "Ingresos Totales" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Value = headers[i];
                    worksheet.Cells[row, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[row, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                row++;
                
                // Datos de géneros
                for (int i = 0; i < model.Count; i++)
                {
                    var genero = model[i];
                    
                    // #
                    worksheet.Cells[row, 1].Value = i + 1;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Género
                    worksheet.Cells[row, 2].Value = genero.Genero;
                    
                    // Películas
                    worksheet.Cells[row, 3].Value = genero.CantidadPeliculas;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Tickets Vendidos
                    worksheet.Cells[row, 4].Value = genero.TotalTickets;
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // % del Total
                    worksheet.Cells[row, 5].Value = genero.PorcentajeVentas / 100; // Como decimal para formato porcentaje
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "0.0%";
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Ingresos Totales
                    worksheet.Cells[row, 6].Value = genero.IngresosTotales;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    // Aplicar bordes a todas las celdas de la fila
                    for (int j = 1; j <= 6; j++)
                    {
                        worksheet.Cells[row, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    
                    // Destacar los 3 primeros géneros
                    if (i < 3)
                    {
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        worksheet.Cells[row, 2].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    }
                    
                    row++;
                }
                
                // Fila de totales
                worksheet.Cells[row, 1].Value = "TOTALES";
                worksheet.Cells[row, 1, row, 2].Merge = true;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                
                worksheet.Cells[row, 3].Value = totalPeliculas;
                worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 3].Style.Font.Bold = true;
                
                worksheet.Cells[row, 4].Value = totalTickets;
                worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 4].Style.Font.Bold = true;
                
                worksheet.Cells[row, 5].Value = 1; // 100%
                worksheet.Cells[row, 5].Style.Numberformat.Format = "0%";
                worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 5].Style.Font.Bold = true;
                
                worksheet.Cells[row, 6].Value = totalIngresos;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 6].Style.Font.Bold = true;
                
                // Aplicar sombreado y bordes a la fila de totales
                for (int i = 1; i <= 6; i++)
                {
                    worksheet.Cells[row, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                
                // Crear gráfico pie chart
                row += 3;
                
                // Skip chart creation and just add a note
                worksheet.Cells[row, 1].Value = "Nota: Para visualizar gráficos, consulte el reporte en la aplicación web.";
                worksheet.Cells[row, 1, row, 6].Merge = true;
                worksheet.Cells[row, 1].Style.Font.Italic = true;
                
                // Ajustar ancho de columnas
                worksheet.Cells.AutoFitColumns();
                
                // Generar archivo
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        public async Task<FileResult> ExportReporteVentasDiarias(List<ReporteVentaDiariaViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Ventas_Diarias_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            // Configure EPPlus license for version 8+
            try
            {
                // For EPPlus version 8+, use the new License approach
                ExcelPackage.License.SetNonCommercialOrganization("CineCenter");
            }
            catch
            {
                // For EPPlus versions 5-7
                try
                {
                    var licenseContextProperty = typeof(ExcelPackage).GetProperty("LicenseContext");
                    if (licenseContextProperty != null)
                    {
                        var nonCommercialValue = Enum.Parse(licenseContextProperty.PropertyType, "NonCommercial");
                        licenseContextProperty.SetValue(null, nonCommercialValue);
                    }
                }
                catch
                {
                    // Fallback if both approaches fail
                }
            }
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Ventas Diarias");
                
                // Título del reporte
                worksheet.Cells[1, 1].Value = "REPORTE DE VENTAS DIARIAS";
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Fecha del reporte
                worksheet.Cells[2, 1].Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                worksheet.Cells[2, 1, 2, 6].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Italic = true;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Período del reporte
                worksheet.Cells[3, 1].Value = $"Período: {model.Min(v => v.Fecha):dd/MM/yyyy} - {model.Max(v => v.Fecha):dd/MM/yyyy}";
                worksheet.Cells[3, 1, 3, 6].Merge = true;
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Datos de resumen
                var totalTickets = model.Sum(v => v.TotalTickets);
                var totalIngresos = model.Sum(v => v.IngresosTotales);
                var totalFunciones = model.Sum(v => v.CantidadFunciones);
                var promedioPrecio = totalTickets > 0 ? totalIngresos / totalTickets : 0;
                
                int row = 5;
                
                // Resumen
                worksheet.Cells[row, 1].Value = "Total de Tickets Vendidos:";
                worksheet.Cells[row, 2].Value = totalTickets;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Total de Funciones:";
                worksheet.Cells[row, 2].Value = totalFunciones;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Ingresos Totales:";
                worksheet.Cells[row, 2].Value = totalIngresos;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Precio Promedio por Ticket:";
                worksheet.Cells[row, 2].Value = promedioPrecio;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row += 2;
                
                // Encabezados de tabla
                var headers = new[] { "Fecha", "Tickets Vendidos", "Funciones", "Promedio por Función", "Precio Promedio", "Ingresos" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Value = headers[i];
                    worksheet.Cells[row, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[row, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                row++;
                
                // Datos de ventas diarias
                foreach (var venta in model.OrderByDescending(v => v.Fecha))
                {
                    // Fecha
                    worksheet.Cells[row, 1].Value = venta.Fecha;
                    worksheet.Cells[row, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Tickets Vendidos
                    worksheet.Cells[row, 2].Value = venta.TotalTickets;
                    worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Funciones
                    worksheet.Cells[row, 3].Value = venta.CantidadFunciones;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Promedio por Función
                    var promedioFuncion = venta.CantidadFunciones > 0 ? (double)venta.TotalTickets / venta.CantidadFunciones : 0;
                    worksheet.Cells[row, 4].Value = promedioFuncion;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "0.0";
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Precio Promedio
                    worksheet.Cells[row, 5].Value = venta.PromedioTicketPrecio;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Ingresos Totales
                    worksheet.Cells[row, 6].Value = venta.IngresosTotales;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    // Aplicar bordes a todas las celdas de la fila
                    for (int i = 1; i <= 6; i++)
                    {
                        worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    
                    row++;
                }
                
                // Fila de totales
                worksheet.Cells[row, 1].Value = "TOTALES";
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                
                worksheet.Cells[row, 2].Value = totalTickets;
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 2].Style.Font.Bold = true;
                
                worksheet.Cells[row, 3].Value = totalFunciones;
                worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 3].Style.Font.Bold = true;
                
                var promedioTotalFuncion = totalFunciones > 0 ? (double)totalTickets / totalFunciones : 0;
                worksheet.Cells[row, 4].Value = promedioTotalFuncion;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "0.0";
                worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 4].Style.Font.Bold = true;
                
                worksheet.Cells[row, 5].Value = promedioPrecio;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 5].Style.Font.Bold = true;
                
                worksheet.Cells[row, 6].Value = totalIngresos;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 6].Style.Font.Bold = true;
                
                // Aplicar sombreado y bordes a la fila de totales
                for (int i = 1; i <= 6; i++)
                {
                    worksheet.Cells[row, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                
                // Crear gráfico de tendencia
                row += 3;
                
                // Agregar una nota ya que no vamos a implementar el gráfico para evitar problemas con la versión de EPPlus
                worksheet.Cells[row, 1].Value = "Nota: Para visualizar gráficos, consulte el reporte en la aplicación web.";
                worksheet.Cells[row, 1, row, 6].Merge = true;
                worksheet.Cells[row, 1].Style.Font.Italic = true;
                
                // Ajustar ancho de columnas
                worksheet.Cells.AutoFitColumns();
                
                // Generar archivo
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        public async Task<FileResult> ExportReporteFormatos(List<ReporteFormatoViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Formatos_Preferidos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            // Configure EPPlus license for version 8+
            try
            {
                // For EPPlus version 8+, use the new License approach
                ExcelPackage.License.SetNonCommercialOrganization("CineCenter");
            }
            catch
            {
                // For EPPlus versions 5-7
                try
                {
                    var licenseContextProperty = typeof(ExcelPackage).GetProperty("LicenseContext");
                    if (licenseContextProperty != null)
                    {
                        var nonCommercialValue = Enum.Parse(licenseContextProperty.PropertyType, "NonCommercial");
                        licenseContextProperty.SetValue(null, nonCommercialValue);
                    }
                }
                catch
                {
                    // Fallback if both approaches fail
                }
            }
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Formatos Preferidos");
                
                // Título del reporte
                worksheet.Cells[1, 1].Value = "REPORTE DE FORMATOS PREFERIDOS";
                worksheet.Cells[1, 1, 1, 5].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Fecha del reporte
                worksheet.Cells[2, 1].Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                worksheet.Cells[2, 1, 2, 5].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Italic = true;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Datos de resumen
                var totalTickets = model.Sum(f => f.TotalTickets);
                var totalIngresos = model.Sum(f => f.IngresosTotales);
                var totalPeliculas = model.Sum(f => f.CantidadPeliculas);
                
                int row = 4;
                
                // Resumen
                worksheet.Cells[row, 1].Value = "Total de Formatos:";
                worksheet.Cells[row, 2].Value = model.Count;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Total de Películas:";
                worksheet.Cells[row, 2].Value = totalPeliculas;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Total de Tickets Vendidos:";
                worksheet.Cells[row, 2].Value = totalTickets;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row++;
                
                worksheet.Cells[row, 1].Value = "Ingresos Totales:";
                worksheet.Cells[row, 2].Value = totalIngresos;
                worksheet.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                row += 2;
                
                // Encabezados de tabla
                var headers = new[] { "Formato", "Películas", "Tickets Vendidos", "% Preferencia", "Ingresos Totales" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Value = headers[i];
                    worksheet.Cells[row, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[row, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[row, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                row++;
                
                // Datos de formatos
                foreach (var formato in model)
                {
                    // Formato
                    worksheet.Cells[row, 1].Value = formato.Formato;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Películas
                    worksheet.Cells[row, 2].Value = formato.CantidadPeliculas;
                    worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Tickets Vendidos
                    worksheet.Cells[row, 3].Value = formato.TotalTickets;
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // % Preferencia
                    worksheet.Cells[row, 4].Value = formato.PorcentajePreferencia / 100; // Convertir a decimal para formato porcentaje
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "0.0%";
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    
                    // Ingresos Totales
                    worksheet.Cells[row, 5].Value = formato.IngresosTotales;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    // Aplicar bordes a todas las celdas de la fila
                    for (int i = 1; i <= 5; i++)
                    {
                        worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    
                    row++;
                }
                
                // Fila de totales
                worksheet.Cells[row, 1].Value = "TOTALES";
                worksheet.Cells[row, 1, row, 2].Merge = true;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                
                worksheet.Cells[row, 3].Value = totalTickets;
                worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 3].Style.Font.Bold = true;
                
                worksheet.Cells[row, 4].Value = 1; // 100%
                worksheet.Cells[row, 4].Style.Numberformat.Format = "0%";
                worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 4].Style.Font.Bold = true;
                
                worksheet.Cells[row, 5].Value = totalIngresos;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 5].Style.Font.Bold = true;
                
                // Aplicar sombreado y bordes a la fila de totales
                for (int i = 1; i <= 5; i++)
                {
                    worksheet.Cells[row, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                
                // Ajustar ancho de columnas
                worksheet.Cells.AutoFitColumns();
                
                // Generar archivo
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                
                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }
    }
}
