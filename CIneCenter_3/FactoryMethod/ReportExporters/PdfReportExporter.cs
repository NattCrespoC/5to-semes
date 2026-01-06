using ASP_MVC_Prueba.Interfaces;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ASP_MVC_Prueba.FactoryMethod.ReportExporters
{
    public class PdfReportExporter : IReportExporter
    {
        public async Task<FileResult> ExportReportePelisGanancia(ResumenReporteViewModel model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Peliculas_Ganancias_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 60, 60);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Agregar título
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Reporte de Ganancias por Película", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));
            
            // Agregar resumen
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            
            document.Add(new Paragraph($"Total de Películas: {model.TotalPeliculas}", boldFont));
            document.Add(new Paragraph($"Total de Tickets Vendidos: {model.TotalTicketsGlobal}", boldFont));
            document.Add(new Paragraph($"Ingresos Totales: ${model.IngresosTotalesGlobal:N2}", boldFont));
            document.Add(new Paragraph("\n"));
            
            // Crear tabla
            var table = new PdfPTable(3) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 5f, 2f, 3f });
            
            // Encabezados
            var headers = new[] { "Título", "Tickets", "Ingresos ($)" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, boldFont));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                table.AddCell(cell);
            }
            
            // Datos
            foreach (var pelicula in model.DetallesPeliculas)
            {
                table.AddCell(new PdfPCell(new Phrase(pelicula.Titulo, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(pelicula.TotalTicketsVendidos.ToString(), normalFont)) 
                    { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase($"${pelicula.IngresosTotales:N2}", normalFont)) 
                    { HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            
            document.Add(table);
            document.Add(new Paragraph($"\nReporte generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont));
            
            document.Close();
            writer.Close();
            
            return new FileContentResult(memoryStream.ToArray(), "application/pdf")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<FileResult> ExportReporteGeneros(List<ReporteGeneroViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Generos_Populares_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 60, 60);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Agregar título
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Reporte de Géneros Más Populares", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));
            
            // Agregar resumen
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            
            // Datos de resumen
            var totalGeneros = model.Count;
            var totalTickets = model.Sum(g => g.TotalTickets);
            var totalIngresos = model.Sum(g => g.IngresosTotales);
            var totalPeliculas = model.Sum(g => g.CantidadPeliculas);
            
            document.Add(new Paragraph($"Total de Géneros: {totalGeneros}", boldFont));
            document.Add(new Paragraph($"Total de Películas: {totalPeliculas}", boldFont));
            document.Add(new Paragraph($"Total de Tickets Vendidos: {totalTickets}", boldFont));
            document.Add(new Paragraph($"Ingresos Totales: ${totalIngresos:N2}", boldFont));
            document.Add(new Paragraph("\n"));
            
            // Top 3 géneros
            document.Add(new Paragraph("Top 3 Géneros Más Populares:", boldFont));
            foreach (var genero in model.Take(3))
            {
                document.Add(new Paragraph($"• {genero.Genero}: {genero.PorcentajeVentas:N1}% del total ({genero.TotalTickets} tickets)", normalFont));
            }
            document.Add(new Paragraph("\n"));
            
            // Crear tabla
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1f, 4f, 2f, 2f, 3f });
            
            // Encabezados
            var headers = new[] { "#", "Género", "Películas", "Tickets", "Ingresos ($)" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, boldFont));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                table.AddCell(cell);
            }
            
            // Datos
            int index = 1;
            foreach (var genero in model)
            {
                // Columna #
                var indexCell = new PdfPCell(new Phrase(index.ToString(), normalFont));
                indexCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(indexCell);
                
                // Columna Género
                var generoCell = new PdfPCell(new Phrase(genero.Genero, normalFont));
                table.AddCell(generoCell);
                
                // Columna Películas
                var peliculasCell = new PdfPCell(new Phrase(genero.CantidadPeliculas.ToString(), normalFont));
                peliculasCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(peliculasCell);
                
                // Columna Tickets
                var ticketsCell = new PdfPCell(new Phrase(genero.TotalTickets.ToString(), normalFont));
                ticketsCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(ticketsCell);
                
                // Columna Ingresos
                var ingresosCell = new PdfPCell(new Phrase($"${genero.IngresosTotales:N2}", normalFont));
                ingresosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(ingresosCell);
                
                index++;
            }
            
            // Agregar fila de totales
            var totalLabel = new PdfPCell(new Phrase("TOTALES", boldFont));
            totalLabel.Colspan = 2;
            totalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabel.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalLabel);
            
            var totalPeliculasCell = new PdfPCell(new Phrase(totalPeliculas.ToString(), boldFont));
            totalPeliculasCell.HorizontalAlignment = Element.ALIGN_CENTER;
            totalPeliculasCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalPeliculasCell);
            
            var totalTicketsCell = new PdfPCell(new Phrase(totalTickets.ToString(), boldFont));
            totalTicketsCell.HorizontalAlignment = Element.ALIGN_CENTER;
            totalTicketsCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalTicketsCell);
            
            var totalIngresosCell = new PdfPCell(new Phrase($"${totalIngresos:N2}", boldFont));
            totalIngresosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalIngresosCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalIngresosCell);
            
            document.Add(table);
            
            // Agregar gráfico (como texto descriptivo, ya que iTextSharp no genera gráficos)
            document.Add(new Paragraph("\nNota: La versión en PDF no incluye gráficos. Para visualizar el gráfico de distribución de géneros, consulte la versión web o descargue el formato Excel.", normalFont));
            
            // Agregar fecha de generación
            document.Add(new Paragraph($"\nReporte generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont));
            
            document.Close();
            writer.Close();
            
            return new FileContentResult(memoryStream.ToArray(), "application/pdf")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<FileResult> ExportReporteVentasDiarias(List<ReporteVentaDiariaViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Ventas_Diarias_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 60, 60);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Agregar título
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Reporte de Ventas Diarias", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));
            
            // Agregar resumen
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            
            // Datos de resumen
            var totalTickets = model.Sum(v => v.TotalTickets);
            var totalIngresos = model.Sum(v => v.IngresosTotales);
            var totalFunciones = model.Sum(v => v.CantidadFunciones);
            var promedioPrecio = totalTickets > 0 ? totalIngresos / totalTickets : 0;
            
            document.Add(new Paragraph($"Período: {model.Min(v => v.Fecha):dd/MM/yyyy} - {model.Max(v => v.Fecha):dd/MM/yyyy}", boldFont));
            document.Add(new Paragraph($"Total de Tickets Vendidos: {totalTickets}", boldFont));
            document.Add(new Paragraph($"Total de Funciones: {totalFunciones}", boldFont));
            document.Add(new Paragraph($"Ingresos Totales: ${totalIngresos:N2}", boldFont));
            document.Add(new Paragraph($"Precio Promedio por Ticket: ${promedioPrecio:N2}", boldFont));
            document.Add(new Paragraph("\n"));
            
            // Crear tabla
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2.5f, 1.5f, 1.5f, 2f, 2.5f });
            
            // Encabezados
            var headers = new[] { "Fecha", "Tickets", "Funciones", "Precio Promedio", "Ingresos ($)" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, boldFont));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                table.AddCell(cell);
            }
            
            // Datos
            foreach (var venta in model)
            {
                // Columna Fecha
                var fechaCell = new PdfPCell(new Phrase(venta.Fecha.ToString("dd/MM/yyyy"), normalFont));
                fechaCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(fechaCell);
                
                // Columna Tickets
                var ticketsCell = new PdfPCell(new Phrase(venta.TotalTickets.ToString(), normalFont));
                ticketsCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(ticketsCell);
                
                // Columna Funciones
                var funcionesCell = new PdfPCell(new Phrase(venta.CantidadFunciones.ToString(), normalFont));
                funcionesCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(funcionesCell);
                
                // Columna Precio Promedio
                var precioCell = new PdfPCell(new Phrase($"${venta.PromedioTicketPrecio:N2}", normalFont));
                precioCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(precioCell);
                
                // Columna Ingresos
                var ingresosCell = new PdfPCell(new Phrase($"${venta.IngresosTotales:N2}", normalFont));
                ingresosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(ingresosCell);
            }
            
            // Agregar fila de totales
            var totalLabel = new PdfPCell(new Phrase("TOTALES", boldFont));
            totalLabel.HorizontalAlignment = Element.ALIGN_CENTER;
            totalLabel.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalLabel);
            
            var totalTicketsCell = new PdfPCell(new Phrase(totalTickets.ToString(), boldFont));
            totalTicketsCell.HorizontalAlignment = Element.ALIGN_CENTER;
            totalTicketsCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalTicketsCell);
            
            var totalFuncionesCell = new PdfPCell(new Phrase(totalFunciones.ToString(), boldFont));
            totalFuncionesCell.HorizontalAlignment = Element.ALIGN_CENTER;
            totalFuncionesCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalFuncionesCell);
            
            var promedioPrecioCell = new PdfPCell(new Phrase($"${promedioPrecio:N2}", boldFont));
            promedioPrecioCell.HorizontalAlignment = Element.ALIGN_CENTER;
            promedioPrecioCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(promedioPrecioCell);
            
            var totalIngresosCell = new PdfPCell(new Phrase($"${totalIngresos:N2}", boldFont));
            totalIngresosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalIngresosCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalIngresosCell);
            
            document.Add(table);
            
            // Agregar gráfico (como texto descriptivo, ya que iTextSharp no genera gráficos)
            document.Add(new Paragraph("\nNota: La versión en PDF no incluye gráficos. Para visualizar el gráfico de tendencia de ventas, consulte la versión web o descargue el formato Excel.", normalFont));
            
            // Agregar fecha de generación
            document.Add(new Paragraph($"\nReporte generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont));
            
            document.Close();
            writer.Close();
            
            return new FileContentResult(memoryStream.ToArray(), "application/pdf")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<FileResult> ExportReporteFormatos(List<ReporteFormatoViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Formatos_Preferidos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 60, 60);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Agregar título
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Reporte de Formatos Preferidos", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));
            
            // Agregar resumen
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            
            // Datos de resumen
            var totalTickets = model.Sum(f => f.TotalTickets);
            var totalIngresos = model.Sum(f => f.IngresosTotales);
            var totalPeliculas = model.Sum(f => f.CantidadPeliculas);
            
            document.Add(new Paragraph($"Total de Formatos: {model.Count}", boldFont));
            document.Add(new Paragraph($"Total de Películas: {totalPeliculas}", boldFont));
            document.Add(new Paragraph($"Total de Tickets Vendidos: {totalTickets}", boldFont));
            document.Add(new Paragraph($"Ingresos Totales: ${totalIngresos:N2}", boldFont));
            document.Add(new Paragraph("\n"));
            
            // Crear tabla
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 3f });
            
            // Encabezados
            var headers = new[] { "Formato", "Películas", "Tickets", "% Preferencia", "Ingresos ($)" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, boldFont));
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                table.AddCell(cell);
            }
            
            // Datos
            foreach (var formato in model)
            {
                // Columna Formato
                var formatoCell = new PdfPCell(new Phrase(formato.Formato, normalFont));
                formatoCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(formatoCell);
                
                // Columna Películas
                var peliculasCell = new PdfPCell(new Phrase(formato.CantidadPeliculas.ToString(), normalFont));
                peliculasCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(peliculasCell);
                
                // Columna Tickets
                var ticketsCell = new PdfPCell(new Phrase(formato.TotalTickets.ToString(), normalFont));
                ticketsCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(ticketsCell);
                
                // Columna % Preferencia
                var porcentajeCell = new PdfPCell(new Phrase($"{formato.PorcentajePreferencia:N1}%", normalFont));
                porcentajeCell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(porcentajeCell);
                
                // Columna Ingresos
                var ingresosCell = new PdfPCell(new Phrase($"${formato.IngresosTotales:N2}", normalFont));
                ingresosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(ingresosCell);
            }
            
            // Agregar fila de totales
            var totalLabel = new PdfPCell(new Phrase("TOTALES", boldFont));
            totalLabel.Colspan = 2;
            totalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalLabel.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalLabel);
            
            var totalTicketsCell = new PdfPCell(new Phrase(totalTickets.ToString(), boldFont));
            totalTicketsCell.HorizontalAlignment = Element.ALIGN_CENTER;
            totalTicketsCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalTicketsCell);
            
            var totalPorcentajeCell = new PdfPCell(new Phrase("100%", boldFont));
            totalPorcentajeCell.HorizontalAlignment = Element.ALIGN_CENTER;
            totalPorcentajeCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalPorcentajeCell);
            
            var totalIngresosCell = new PdfPCell(new Phrase($"${totalIngresos:N2}", boldFont));
            totalIngresosCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalIngresosCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(totalIngresosCell);
            
            document.Add(table);
            
            // Agregar fecha de generación
            document.Add(new Paragraph($"\nReporte generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont));
            
            document.Close();
            writer.Close();
            
            return new FileContentResult(memoryStream.ToArray(), "application/pdf")
            {
                FileDownloadName = fileName
            };
        }
    }
}
