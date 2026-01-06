using ASP_MVC_Prueba.FactoryMethod;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Service
{
    public class ReportesService
    {
        private readonly CineCenterContext _context;
        public ReportesService(CineCenterContext context)
        {
            _context = context;
        }
        
        // Obtiene un reporte de las peliculas con el total de tickets vendidos y los ingresos totales
        public async Task<List<object>> GetReportesAsync()
        {
            var reportes = await _context.Peliculas
                .Select(p => new
                {
                    p.Titulo,
                    TotalTicketsVendidos = _context.Tickets.Count(t => t.Horario.PeliculaId == p.Id_Peliculas),
                    IngresosTotales = _context.Tickets
                        .Where(t => t.Horario.PeliculaId == p.Id_Peliculas)
                        .Sum(t => t.Horario.Precio)
                })
                .OrderByDescending(r => r.IngresosTotales)
                .Cast<object>()
                .ToListAsync();
            return reportes;
        }
        
        // Método para exportar reportes
        public async Task<FileResult> ExportReportAsync(ExportReportViewModel exportRequest, object reportData)
        {
            var exporter = ReportExporterFactory.CreateExporter(exportRequest.Format);
            
            switch (exportRequest.ReportType)
            {
                case "PelisGanancia":
                    return await exporter.ExportReportePelisGanancia((ResumenReporteViewModel)reportData);
                
                case "Generos":
                    return await exporter.ExportReporteGeneros((List<ReporteGeneroViewModel>)reportData);
                
                case "VentasDiarias":
                    return await exporter.ExportReporteVentasDiarias((List<ReporteVentaDiariaViewModel>)reportData);
                
                case "Formatos":
                    return await exporter.ExportReporteFormatos((List<ReporteFormatoViewModel>)reportData);
                
                default:
                    throw new ArgumentException($"Tipo de reporte no soportado: {exportRequest.ReportType}");
            }
        }
    }
}
