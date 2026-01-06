using ASP_MVC_Prueba.FactoryMethod.ReportExporters;
using ASP_MVC_Prueba.Interfaces;
using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.FactoryMethod;
using System;

namespace ASP_MVC_Prueba.FactoryMethod
{
    public class ReportExporterFactory
    {
        public static IReportExporter CreateExporter(ReportFormat format)
        {
            return format switch
            {
                ReportFormat.PDF => new PdfReportExporter(),
                ReportFormat.CSV => new CsvReportExporter(),
                ReportFormat.Excel => new ExcelReportExporter(),
                _ => throw new ArgumentException($"Unsupported report format: {format}")
            };
        }
    }
}
