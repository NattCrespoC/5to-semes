using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Service
{
    public class FacturasService : IFacturasService
    {
        private readonly CineCenterContext _context;

        public FacturasService(CineCenterContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Facturas>> GetAllAsync()
        {
            return await _context.Facturas
                .Include(f => f.Ticket)
                .ToListAsync();
        }

        public async Task<Facturas> GetByIdAsync(int id)
        {
            return await _context.Facturas
                .Include(f => f.Ticket)
                .FirstOrDefaultAsync(f => f.Id_Facturas == id);
        }

        public async Task<IEnumerable<Facturas>> GetByTicketIdAsync(int ticketId)
        {
            return await _context.Facturas
                .Where(f => f.TicketId == ticketId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Facturas>> GetByFechaAsync(DateTime fecha)
        {
            return await _context.Facturas
                .Include(f => f.Ticket)
                .Where(f => f.Fecha.Date == fecha.Date)
                .ToListAsync();
        }

        public async Task<Facturas> CreateAsync(Facturas factura)
        {
            factura.Fecha = DateTime.Now;
            if (string.IsNullOrEmpty(factura.CUF))
            {
                factura.CUF = await GenerateCUFAsync(factura.TicketId);
            }
            
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();
            return factura;
        }

        public async Task<Facturas> UpdateAsync(Facturas factura)
        {
            var existingFactura = await _context.Facturas.FindAsync(factura.Id_Facturas);
            if (existingFactura == null)
                return null;

            _context.Entry(existingFactura).CurrentValues.SetValues(factura);
            await _context.SaveChangesAsync();
            
            return existingFactura;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
                return false;

            _context.Facturas.Remove(factura);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateCUFAsync(int ticketId)
        {
            // Get ticket information
            var ticket = await _context.Tickets
                .Include(t => t.Usuario)
                .Include(t => t.Horario)
                .FirstOrDefaultAsync(t => t.Id_Tickets == ticketId);

            if (ticket == null)
                return null;

            // Generate CUF (Código Único de Factura)
            // This is just an example - actual implementation would depend on country-specific requirements
            var now = DateTime.Now;
            var input = $"{ticket.Id_Tickets}-{ticket.Usuario.Id_Usuarios}-{ticket.HorarioId}-{now.Ticks}";
            
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Substring(0, 50); // Use first 50 characters
            }
        }
    }
}
