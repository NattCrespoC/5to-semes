using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Interfaces
{
    public interface IReportExporter
    {
        Task<FileResult> ExportReportePelisGanancia(ResumenReporteViewModel model, string fileName = null);
        Task<FileResult> ExportReporteGeneros(List<ReporteGeneroViewModel> model, string fileName = null);
        Task<FileResult> ExportReporteVentasDiarias(List<ReporteVentaDiariaViewModel> model, string fileName = null);
        Task<FileResult> ExportReporteFormatos(List<ReporteFormatoViewModel> model, string fileName = null);
    }
}
