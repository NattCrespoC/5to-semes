using System;
using System.Collections.Generic;

namespace ASP_MVC_Prueba.Models
{
    // Reporte de horarios más vendidos
    public class ReporteHorarioViewModel
    {
        public Horarios Horario { get; set; }
        public string PeliculaTitulo { get; set; }
        public string SalaNombre { get; set; }
        public string DiaSemana { get; set; }
        public TimeSpan Hora { get; set; }
        public int TotalTickets { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public decimal IngresosTotales { get; set; }
    }

    // Reporte de géneros más populares
    public class ReporteGeneroViewModel
    {
        public string Genero { get; set; }
        public int CantidadPeliculas { get; set; }
        public int TotalTickets { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal PorcentajeVentas { get; set; }
    }

    // Reporte de ventas por período de tiempo
    public class ReporteVentaDiariaViewModel
    {
        public DateTime Fecha { get; set; }
        public int TotalTickets { get; set; }
        public decimal IngresosTotales { get; set; }
        public int CantidadFunciones { get; set; }
        public decimal PromedioTicketPrecio { get; set; }
    }

    // Modelo para representar el formato de exportación de reportes
    public enum ReportFormat
    {
        PDF,
        CSV,
        Excel
    }

    // Modelo genérico para solicitudes de exportación
    public class ExportReportViewModel
    {
        public ReportFormat Format { get; set; }
        public string ReportType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // Reporte de formatos preferidos
    public class ReporteFormatoViewModel
    {
        public string Formato { get; set; }
        public int CantidadPeliculas { get; set; }
        public int TotalTickets { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal PorcentajePreferencia { get; set; }
    }

    // Modelo simplificado para el reporte de horarios populares
    public class DiaPopularViewModel
    {
        public string DiaSemana { get; set; }
        public int TotalTickets { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class HoraPopularViewModel
    {
        public int Hora { get; set; }
        public int TotalTickets { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class ReporteHorarioSimplificadoViewModel
    {
        public List<DiaPopularViewModel> DiasPopulares { get; set; } = new List<DiaPopularViewModel>();
        public List<HoraPopularViewModel> HorasPopulares { get; set; } = new List<HoraPopularViewModel>();
        public int TotalTickets { get; set; }
        public decimal TotalIngresos { get; set; }
    }
}
