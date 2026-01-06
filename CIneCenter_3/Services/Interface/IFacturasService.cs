using ASP_MVC_Prueba.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Interface
{
    public interface IFacturasService
    {
        Task<IEnumerable<Facturas>> GetAllAsync();
        Task<Facturas> GetByIdAsync(int id);
        Task<IEnumerable<Facturas>> GetByTicketIdAsync(int ticketId);
        Task<IEnumerable<Facturas>> GetByFechaAsync(DateTime fecha);
        Task<Facturas> CreateAsync(Facturas factura);
        Task<Facturas> UpdateAsync(Facturas factura);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerateCUFAsync(int ticketId);
    }
    public interface IPagosService
    {
        Task<IEnumerable<Pagos>> GetAllAsync();
        Task<Pagos> GetByIdAsync(int id);
        Task<Pagos> GetByTicketIdAsync(int ticket);
        Task<Pagos> CreateAsync(Pagos pago);
        Task<Pagos> UpdateAsync(Pagos pago);
        Task<bool> DeleteAsync(int id);
    }
}
