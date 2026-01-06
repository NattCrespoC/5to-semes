using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Controllers
{
    [Authorize(Roles = "Admin")]
    public class Reportes : Controller
    {
        private readonly ReportesService _reportesService;
        private readonly CineCenterContext _context;
        
        public Reportes(CineCenterContext context)
        {
            _reportesService = new ReportesService(context);
            _context = context;
        }
        
        public async Task<IActionResult> ReportePelisGanancia()
        {
            try 
            {
                // Obtener datos de películas con tickets y ganancias
                var detallesPeliculas = await _context.Peliculas
                    .Select(p => new ReporteGananciaPeliculaViewModel
                    {
                        Titulo = p.Titulo,
                        TotalTicketsVendidos = p.Horarios.SelectMany(h => h.Tickets).Count(),
                        IngresosTotales = p.Horarios.Sum(h => h.Tickets.Count() * h.Precio)
                    })
                    .Where(p => p.TotalTicketsVendidos > 0) // Solo películas con tickets vendidos
                    .OrderByDescending(p => p.IngresosTotales) // Ordenar por ingresos (mayor a menor)
                    .ToListAsync();
                
                // Crear el modelo de resumen
                var resumenReporte = new ResumenReporteViewModel
                {
                    DetallesPeliculas = detallesPeliculas,
                    TotalTicketsGlobal = detallesPeliculas.Sum(p => p.TotalTicketsVendidos),
                    IngresosTotalesGlobal = detallesPeliculas.Sum(p => p.IngresosTotales),
                    TotalPeliculas = detallesPeliculas.Count
                };
                
                return View(resumenReporte);
            }
            catch (System.Exception ex)
            {
                // Manejar errores
                ViewBag.ErrorMessage = $"Error al generar el reporte: {ex.Message}";
                return View(new ResumenReporteViewModel 
                { 
                    DetallesPeliculas = new List<ReporteGananciaPeliculaViewModel>() 
                });
            }
        }
        
        public async Task<IActionResult> ReporteGenerosMasPopulares()
        {
            try 
            {
                // Calcular el total global de tickets para porcentajes
                var totalTicketsGlobal = await _context.Tickets.CountAsync();
                
                // Obtener géneros más populares por ventas de tickets
                var generosMasPopulares = await _context.Peliculas
                    .GroupBy(p => p.Genero)
                    .Select(g => new ReporteGeneroViewModel
                    {
                        Genero = g.Key,
                        CantidadPeliculas = g.Count(),
                        TotalTickets = g.Sum(p => p.Horarios.SelectMany(h => h.Tickets).Count()),
                        IngresosTotales = g.Sum(p => p.Horarios.Sum(h => h.Tickets.Count() * h.Precio)),
                        PorcentajeVentas = totalTicketsGlobal > 0 
                            ? g.Sum(p => p.Horarios.SelectMany(h => h.Tickets).Count()) / (decimal)totalTicketsGlobal * 100 
                            : 0
                    })
                    .Where(g => g.TotalTickets > 0)
                    .OrderByDescending(g => g.TotalTickets)
                    .ToListAsync();
                
                return View(generosMasPopulares);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al generar el reporte: {ex.Message}";
                return View(new List<ReporteGeneroViewModel>());
            }
        }
        
        public async Task<IActionResult> ReporteVentasDiarias()
        {
            try 
            {
                // Obtener ventas agrupadas por día (últimos 30 días)
                var fechaInicio = DateTime.Now.AddDays(-30);
                
                var ventasDiarias = await _context.Tickets
                    .Where(t => t.FechaCompra >= fechaInicio)
                    .GroupBy(t => t.FechaCompra.Value.Date)
                    .Select(g => new ReporteVentaDiariaViewModel
                    {
                        Fecha = g.Key,
                        TotalTickets = g.Count(),
                        IngresosTotales = g.Sum(t => t.Horario.Precio),
                        CantidadFunciones = g.Select(t => t.HorarioId).Distinct().Count(),
                        PromedioTicketPrecio = g.Count() > 0 ? g.Sum(t => t.Horario.Precio) / g.Count() : 0
                    })
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();
                
                return View(ventasDiarias);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al generar el reporte: {ex.Message}";
                return View(new List<ReporteVentaDiariaViewModel>());
            }
        }
        
        public async Task<IActionResult> ReporteFormatosPreferidos()
        {
            try 
            {
                // Calcular el total global de tickets para porcentajes
                var totalTicketsGlobal = await _context.Tickets.CountAsync();
                
                // Obtener formatos preferidos basados en el tipo de sala
                var formatosPreferidos = await _context.Tickets
                    .Include(t => t.Horario)
                        .ThenInclude(h => h.Sala)
                    .Include(t => t.Horario)
                        .ThenInclude(h => h.Pelicula)
                    .Where(t => t.Horario != null && t.Horario.Sala != null && !string.IsNullOrEmpty(t.Horario.Sala.Tipo))
                    .GroupBy(t => t.Horario.Sala.Tipo)
                    .Select(g => new ReporteFormatoViewModel
                    {
                        Formato = g.Key,
                        TotalTickets = g.Count(),
                        // Calcular la cantidad de películas únicas mostradas en este formato
                        CantidadPeliculas = g.Select(t => t.Horario.PeliculaId).Distinct().Count(),
                        // Calcular ingresos totales por formato
                        IngresosTotales = g.Sum(t => t.Horario.Precio),
                        // Calcular porcentaje de preferencia
                        PorcentajePreferencia = totalTicketsGlobal > 0 
                            ? (decimal)g.Count() / totalTicketsGlobal * 100 
                            : 0
                    })
                    .OrderByDescending(f => f.TotalTickets)
                    .ToListAsync();
                
                // Manejar el caso en que no hay datos
                if (!formatosPreferidos.Any())
                {
                    // Crear entradas vacías para los tres formatos principales
                    formatosPreferidos = new List<ReporteFormatoViewModel>
                    {
                        new ReporteFormatoViewModel { Formato = "2D", TotalTickets = 0, CantidadPeliculas = 0, IngresosTotales = 0, PorcentajePreferencia = 0 },
                        new ReporteFormatoViewModel { Formato = "3D", TotalTickets = 0, CantidadPeliculas = 0, IngresosTotales = 0, PorcentajePreferencia = 0 },
                        new ReporteFormatoViewModel { Formato = "4D", TotalTickets = 0, CantidadPeliculas = 0, IngresosTotales = 0, PorcentajePreferencia = 0 }
                    };
                }
                else
                {
                    // Asegurar que tenemos datos para los tres formatos principales (2D, 3D, 4D)
                    var formatosConocidos = new[] { "2D", "3D", "4D" };
                    foreach (var formato in formatosConocidos)
                    {
                        if (!formatosPreferidos.Any(f => f.Formato == formato))
                        {
                            formatosPreferidos.Add(new ReporteFormatoViewModel
                            {
                                Formato = formato,
                                TotalTickets = 0,
                                CantidadPeliculas = 0,
                                IngresosTotales = 0,
                                PorcentajePreferencia = 0
                            });
                        }
                    }
                }
                
                return View(formatosPreferidos.OrderByDescending(f => f.TotalTickets).ToList());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al generar el reporte: {ex.Message}";
                return View(new List<ReporteFormatoViewModel>());
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ExportReporte(ExportReportViewModel exportRequest)
        {
            try
            {
                // Obtener los datos del reporte según el tipo solicitado
                object reportData = null;
                
                switch (exportRequest.ReportType)
                {
                    case "PelisGanancia":
                        var detallesPeliculas = await _context.Peliculas
                            .Select(p => new ReporteGananciaPeliculaViewModel
                            {
                                Titulo = p.Titulo,
                                TotalTicketsVendidos = p.Horarios.SelectMany(h => h.Tickets).Count(),
                                IngresosTotales = p.Horarios.Sum(h => h.Tickets.Count() * h.Precio)
                            })
                            .Where(p => p.TotalTicketsVendidos > 0)
                            .OrderByDescending(p => p.IngresosTotales)
                            .ToListAsync();
                            
                        reportData = new ResumenReporteViewModel
                        {
                            DetallesPeliculas = detallesPeliculas,
                            TotalTicketsGlobal = detallesPeliculas.Sum(p => p.TotalTicketsVendidos),
                            IngresosTotalesGlobal = detallesPeliculas.Sum(p => p.IngresosTotales),
                            TotalPeliculas = detallesPeliculas.Count
                        };
                        break;
                        
                    case "Generos":
                        var totalTicketsGlobal = await _context.Tickets.CountAsync();
                        
                        reportData = await _context.Peliculas
                            .GroupBy(p => p.Genero)
                            .Select(g => new ReporteGeneroViewModel
                            {
                                Genero = g.Key,
                                CantidadPeliculas = g.Count(),
                                TotalTickets = g.Sum(p => p.Horarios.SelectMany(h => h.Tickets).Count()),
                                IngresosTotales = g.Sum(p => p.Horarios.Sum(h => h.Tickets.Count() * h.Precio)),
                                PorcentajeVentas = totalTicketsGlobal > 0 
                                    ? g.Sum(p => p.Horarios.SelectMany(h => h.Tickets).Count()) / (decimal)totalTicketsGlobal * 100 
                                    : 0
                            })
                            .Where(g => g.TotalTickets > 0)
                            .OrderByDescending(g => g.TotalTickets)
                            .ToListAsync();
                        break;
                        
                    case "VentasDiarias":
                        var fechaInicio = DateTime.Now.AddDays(-30);
                        
                        reportData = await _context.Tickets
                            .Where(t => t.FechaCompra >= fechaInicio)
                            .GroupBy(t => t.FechaCompra.Value.Date)
                            .Select(g => new ReporteVentaDiariaViewModel
                            {
                                Fecha = g.Key,
                                TotalTickets = g.Count(),
                                IngresosTotales = g.Sum(t => t.Horario.Precio),
                                CantidadFunciones = g.Select(t => t.HorarioId).Distinct().Count(),
                                PromedioTicketPrecio = g.Count() > 0 ? g.Sum(t => t.Horario.Precio) / g.Count() : 0
                            })
                            .OrderByDescending(v => v.Fecha)
                            .ToListAsync();
                        break;
                        
                    case "Formatos":
                        var totalTickets = await _context.Tickets.CountAsync();
                        
                        reportData = await _context.Tickets
                            .Include(t => t.Horario)
                                .ThenInclude(h => h.Sala)
                            .Include(t => t.Horario)
                                .ThenInclude(h => h.Pelicula)
                            .Where(t => t.Horario != null && t.Horario.Sala != null && !string.IsNullOrEmpty(t.Horario.Sala.Tipo))
                            .GroupBy(t => t.Horario.Sala.Tipo)
                            .Select(g => new ReporteFormatoViewModel
                            {
                                Formato = g.Key,
                                TotalTickets = g.Count(),
                                CantidadPeliculas = g.Select(t => t.Horario.PeliculaId).Distinct().Count(),
                                IngresosTotales = g.Sum(t => t.Horario.Precio),
                                PorcentajePreferencia = totalTickets > 0 
                                    ? (decimal)g.Count() / totalTickets * 100 
                                    : 0
                            })
                            .OrderByDescending(f => f.TotalTickets)
                            .ToListAsync();
                        break;
                        
                    default:
                        return BadRequest("Tipo de reporte no soportado");
                }
                
                // Exportar el reporte usando el servicio
                return await _reportesService.ExportReportAsync(exportRequest, reportData);
            }
            catch (Exception ex)
            {
                // Manejar errores
                return BadRequest($"Error al exportar el reporte: {ex.Message}");
            }
        }
    }
}
