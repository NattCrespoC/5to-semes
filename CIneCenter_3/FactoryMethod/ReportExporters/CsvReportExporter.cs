using ASP_MVC_Prueba.Interfaces;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.FactoryMethod.ReportExporters
{
    public class CsvReportExporter : IReportExporter
    {
        public async Task<FileResult> ExportReportePelisGanancia(ResumenReporteViewModel model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Peliculas_Ganancias_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            var sb = new StringBuilder();
            
            // Encabezado CSV
            sb.AppendLine("Título,Tickets Vendidos,Ingresos Totales");
            
            // Datos
            foreach (var pelicula in model.DetallesPeliculas)
            {
                sb.AppendLine($"\"{pelicula.Titulo}\",{pelicula.TotalTicketsVendidos},{pelicula.IngresosTotales:F2}");
            }
            
            // Resumen
            sb.AppendLine();
            sb.AppendLine($"Total Películas,{model.TotalPeliculas}");
            sb.AppendLine($"Total Tickets,{model.TotalTicketsGlobal}");
            sb.AppendLine($"Ingresos Totales,{model.IngresosTotalesGlobal:F2}");
            
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<FileResult> ExportReporteGeneros(List<ReporteGeneroViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Generos_Populares_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            var sb = new StringBuilder();
            
            // Agregar título y fecha
            sb.AppendLine("REPORTE DE GÉNEROS MÁS POPULARES");
            sb.AppendLine($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();
            
            // Agregar resumen
            var totalGeneros = model.Count;
            var totalTickets = model.Sum(g => g.TotalTickets);
            var totalIngresos = model.Sum(g => g.IngresosTotales);
            var totalPeliculas = model.Sum(g => g.CantidadPeliculas);
            
            sb.AppendLine($"Total de Géneros,{totalGeneros}");
            sb.AppendLine($"Total de Películas,{totalPeliculas}");
            sb.AppendLine($"Total de Tickets Vendidos,{totalTickets}");
            sb.AppendLine($"Ingresos Totales,{totalIngresos:F2}");
            sb.AppendLine();
            
            // Top 3 géneros
            sb.AppendLine("TOP 3 GÉNEROS MÁS POPULARES");
            int position = 1;
            foreach (var genero in model.Take(3))
            {
                sb.AppendLine($"{position},{genero.Genero},{genero.PorcentajeVentas:F1}%,{genero.TotalTickets} tickets");
                position++;
            }
            sb.AppendLine();
            
            // Encabezados CSV
            sb.AppendLine("Posición,Género,Películas,Tickets Vendidos,% del Total,Ingresos Totales");
            
            // Datos
            position = 1;
            foreach (var genero in model)
            {
                sb.AppendLine($"{position},\"{genero.Genero}\",{genero.CantidadPeliculas},{genero.TotalTickets},{genero.PorcentajeVentas:F1}%,{genero.IngresosTotales:F2}");
                position++;
            }
            
            // Totales
            sb.AppendLine($"TOTALES,,{totalPeliculas},{totalTickets},100%,{totalIngresos:F2}");
            
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<FileResult> ExportReporteVentasDiarias(List<ReporteVentaDiariaViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Ventas_Diarias_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            var sb = new StringBuilder();
            
            // Agregar título y fecha
            sb.AppendLine("REPORTE DE VENTAS DIARIAS");
            sb.AppendLine($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Período: {model.Min(v => v.Fecha):dd/MM/yyyy} - {model.Max(v => v.Fecha):dd/MM/yyyy}");
            sb.AppendLine();
            
            // Agregar resumen
            var totalTickets = model.Sum(v => v.TotalTickets);
            var totalIngresos = model.Sum(v => v.IngresosTotales);
            var totalFunciones = model.Sum(v => v.CantidadFunciones);
            var promedioPrecio = totalTickets > 0 ? totalIngresos / totalTickets : 0;
            
            sb.AppendLine($"Total de Tickets Vendidos,{totalTickets}");
            sb.AppendLine($"Total de Funciones,{totalFunciones}");
            sb.AppendLine($"Ingresos Totales,{totalIngresos:F2}");
            sb.AppendLine($"Precio Promedio por Ticket,{promedioPrecio:F2}");
            sb.AppendLine();
            
            // Encabezados CSV
            sb.AppendLine("Fecha,Tickets Vendidos,Funciones,Promedio por Función,Precio Promedio,Ingresos Totales");
            
            // Datos
            foreach (var venta in model.OrderByDescending(v => v.Fecha))
            {
                var promedioFuncion = venta.CantidadFunciones > 0 ? (double)venta.TotalTickets / venta.CantidadFunciones : 0;
                sb.AppendLine($"{venta.Fecha:yyyy-MM-dd},{venta.TotalTickets},{venta.CantidadFunciones},{promedioFuncion:F1},{venta.PromedioTicketPrecio:F2},{venta.IngresosTotales:F2}");
            }
            
            // Totales
            sb.AppendLine();
            var promedioTotalFuncion = totalFunciones > 0 ? (double)totalTickets / totalFunciones : 0;
            sb.AppendLine($"TOTALES,{totalTickets},{totalFunciones},{promedioTotalFuncion:F1},{promedioPrecio:F2},{totalIngresos:F2}");
            
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<FileResult> ExportReporteFormatos(List<ReporteFormatoViewModel> model, string fileName = null)
        {
            if (fileName == null) fileName = $"Reporte_Formatos_Preferidos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            var sb = new StringBuilder();
            
            // Agregar título y fecha
            sb.AppendLine("REPORTE DE FORMATOS PREFERIDOS");
            sb.AppendLine($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();
            
            // Agregar resumen
            var totalTickets = model.Sum(f => f.TotalTickets);
            var totalIngresos = model.Sum(f => f.IngresosTotales);
            var totalPeliculas = model.Sum(f => f.CantidadPeliculas);
            
            sb.AppendLine($"Total de Formatos,{model.Count}");
            sb.AppendLine($"Total de Películas,{totalPeliculas}");
            sb.AppendLine($"Total de Tickets Vendidos,{totalTickets}");
            sb.AppendLine($"Ingresos Totales,{totalIngresos:F2}");
            sb.AppendLine();
            
            // Encabezados CSV
            sb.AppendLine("Formato,Películas,Tickets Vendidos,% Preferencia,Ingresos Totales");
            
            // Datos
            foreach (var formato in model)
            {
                sb.AppendLine($"{formato.Formato},{formato.CantidadPeliculas},{formato.TotalTickets},{formato.PorcentajePreferencia:F1}%,{formato.IngresosTotales:F2}");
            }
            
            // Totales
            sb.AppendLine($"TOTALES,{totalPeliculas},{totalTickets},100%,{totalIngresos:F2}");
            
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            return new FileContentResult(bytes, "text/csv")
            {
                FileDownloadName = fileName
            };
        }
    }
}
