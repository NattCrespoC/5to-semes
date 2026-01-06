using ASP_MVC_Prueba.Controllers;
using ASP_MVC_Prueba.Entidad;
using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Service
{
    public class TicketsService : ITicketsService
    {
        private readonly CineCenterContext _context;

        public TicketsService(CineCenterContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tickets>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Horario)
                    .ThenInclude(h => h.Sala)
                .Include(t => t.Usuario)
                .ToListAsync();
        }

        public async Task<Tickets> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Horario)
                    .ThenInclude(h => h.Pelicula)
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Id_Tickets == id);
        }

        public async Task<IEnumerable<Tickets>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Tickets
                .Include(t => t.Horario)
                    .ThenInclude(h => h.Pelicula)
                .Include(t => t.Horario)
                    .ThenInclude(h => h.Sala)
                .Include(t => t.Butaca)
                .Where(t => t.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tickets>> GetByHorarioIdAsync(int horarioId)
        {
            return await _context.Tickets
                .Include(t => t.Usuario)
                .Where(t => t.HorarioId == horarioId)
                .ToListAsync();
        }

        public async Task<Tickets> CreateAsync(Tickets ticket)
        {
            ticket.FechaCompra = DateTime.Now;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Tickets> UpdateAsync(Tickets ticket)
        {
            var existingTicket = await _context.Tickets.FindAsync(ticket.Id_Tickets);
            if (existingTicket == null)
                return null;

            _context.Entry(existingTicket).CurrentValues.SetValues(ticket);
            await _context.SaveChangesAsync();

            return existingTicket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateTicketAsync(int id)
        {
            var ticket = await GetByIdAsync(id);
            if (ticket == null)
                return false;

            var currentTime = DateTime.Now;
            return ticket.Horario.FechaHora > currentTime;
        }

        public async Task<Horarios> GetHorarioConTicketsYButacasAsync(int horarioId)
        {
            return await _context.Horarios
                .Where(h => h.Id_Horarios == horarioId)
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .Include(h => h.Tickets)
                    .ThenInclude(t => t.Butaca)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Butacas> GetButacaByIdAsync(int idButaca)
        {
            return await _context.Butacas
                .FirstOrDefaultAsync(b => b.Id_Butacas == idButaca);
        }

        // Nuevos métodos para estadísticas
        public async Task<List<PeliculaTicketsViewModel>> GetPeliculasConEstadisticasAsync()
        {
            return await _context.Peliculas
                .Select(p => new PeliculaTicketsViewModel
                {
                    Pelicula = p,
                    TotalTickets = p.Horarios.SelectMany(h => h.Tickets).Count(),
                    TotalVentas = p.Horarios.Sum(h => h.Tickets.Count() * h.Precio)
                })
                .ToListAsync();
        }

        public async Task<List<HorarioTicketsViewModel>> GetHorariosConEstadisticasByPeliculaIdAsync(int peliculaId)
        {
            return await _context.Horarios
                .Where(h => h.PeliculaId == peliculaId)
                .Select(h => new HorarioTicketsViewModel
                {
                    Horario = h,
                    TotalTickets = h.Tickets.Count(),
                    TotalVentas = h.Tickets.Count() * h.Precio
                })
                .ToListAsync();
        }

        public async Task<List<Tickets>> GetTicketsDetalladosByHorarioIdAsync(int horarioId)
        {
            return await _context.Tickets
                .Include(t => t.Usuario)
                .Include(t => t.Horario)
                .Include(t => t.Butaca)
                .Include(t => t.Pagos)
                .Include(t => t.Facturas)
                .Where(t => t.HorarioId == horarioId)
                .ToListAsync();
        }

        public async Task<(bool Success, string ErrorMessage)> CreateTicketsAsync(CreateTickets createTickets, Horarios horario)
        {
            try
            {
                // Validación simplificada
                if (ValidateTicketCreationFails(createTickets, horario, out string errorMessage))
                    return (false, errorMessage);

                // Usar una transacción para crear todos los tickets
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var butacaId in createTickets.ButacaId)
                    {
                        var ticket = CreateTicketEntity(createTickets, butacaId, horario.Precio);
                        _context.Tickets.Add(ticket);
                        
                        // Ensure Pagos and Facturas are added to their respective DbSets
                        _context.Pagos.Add(ticket.Pagos);
                        _context.Facturas.Add(ticket.Facturas);
                    }
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return (true, null);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al crear tickets: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error interno: {ex.Message}");
            }
        }

        // Método auxiliar para validación
        private bool ValidateTicketCreationFails(CreateTickets createTickets, Horarios horario, out string errorMessage)
        {
            errorMessage = null;
            
            if (createTickets.UsuarioId <= 0)
                errorMessage = "ID de usuario inválido";
            else if (createTickets.ButacaId == null || !createTickets.ButacaId.Any())
                errorMessage = "No se especificaron asientos";
            else if (horario == null)
                errorMessage = "Horario no encontrado";
            
            return errorMessage != null;
        }

        // Método auxiliar para crear entidades de ticket
        private Tickets CreateTicketEntity(CreateTickets createTickets, int butacaId, decimal precio)
        {
            var pagos = new Pagos()
            {
                Estado = "Pagado",
                FechaPago = DateTime.Now,
                Monto = precio,
            };

            var facturas = new Facturas()
            {
                Fecha = DateTime.Now,
                CUF = Guid.NewGuid().ToString(),
            };

            var ticket = new Tickets()
            {
                UsuarioId = createTickets.UsuarioId,
                HorarioId = createTickets.HorarioId,
                ButacaId = butacaId,
                FechaCompra = DateTime.Now,
                Pagos = pagos,
                Facturas = facturas
            };

            return ticket;
        }
    }
}
