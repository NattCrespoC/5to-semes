using System;
using System.Collections.Generic;

namespace ASP_MVC_Prueba.Models
{
    public class ReporteGananciaPeliculaViewModel
    {
        public string Titulo { get; set; }
        public int TotalTicketsVendidos { get; set; }
        public decimal IngresosTotales { get; set; }
    }

    public class ResumenReporteViewModel
    {
        public List<ReporteGananciaPeliculaViewModel> DetallesPeliculas { get; set; }
        public int TotalTicketsGlobal { get; set; }
        public decimal IngresosTotalesGlobal { get; set; }
        public int TotalPeliculas { get; set; }
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }
}
