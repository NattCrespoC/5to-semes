using ASP_MVC_Prueba.Entidad;
using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Service
{
    public class HorariosService : IHorariosService
    {
        private readonly CineCenterContext _context;

        public HorariosService(CineCenterContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Horarios>> GetAllAsync()
        {
            return await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .ToListAsync();
        }

        public async Task<Horarios> GetByIdAsync(int id)
        {
            return await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .FirstOrDefaultAsync(h => h.Id_Horarios == id);
        }

        public async Task<IEnumerable<Horarios>> GetByPeliculaIdAsync(int peliculaId)
        {
            return await _context.Horarios
                .Include(h => h.Sala)
                .Where(h => h.PeliculaId == peliculaId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Horarios>> GetBySalaIdAsync(int salaId)
        {
            return await _context.Horarios
                .Include(h => h.Pelicula)
                .Where(h => h.SalaId == salaId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Horarios>> GetByFechaAsync(DateTime fecha)
        {
            return await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .Where(h => h.FechaHora.Date == fecha.Date)
                .ToListAsync();
        }

        public async Task<Horarios> CreateAsync(Horarios horario)
        {
            _context.Horarios.Add(horario);
            await _context.SaveChangesAsync();
            return horario;
        }

        public async Task<Horarios> UpdateAsync(Horarios horario)
        {
            var existingHorario = await _context.Horarios.FindAsync(horario.Id_Horarios);
            if (existingHorario == null)
                return null;

            _context.Entry(existingHorario).CurrentValues.SetValues(horario);
            await _context.SaveChangesAsync();
            
            return existingHorario;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var horario = await _context.Horarios.FindAsync(id);
            if (horario == null)
                return false;

            _context.Horarios.Remove(horario);
            await _context.SaveChangesAsync();
            return true;
        }

        //funcion para recuperar horarios con sus tickets y las butacas asociadas
        public async Task<IEnumerable<Horarios>> GetHorariosConTicketsYButacasAsync()
        {
            return await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .Include(h => h.Tickets)
                    .ThenInclude(t => t.Butaca)
                .ToListAsync();
        }

        //funcion para recuperar un horario específico con sus tickets y butacas
        public async Task<Horarios> GetHorarioConTicketsYButacasAsync(int horarioId)
        {
            return await _context.Horarios
                .Where(h => h.Id_Horarios == horarioId)
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                    .ThenInclude(s => s.Butacas) // Incluir todas las butacas de la sala
                .Include(h => h.Tickets)
                    .ThenInclude(t => t.Butaca)
                .FirstOrDefaultAsync();
        }
        public async Task<List<AsientosOcupados>> GetAsientosOcupadosAsync(int horarioId)
        {
            return await _context.Tickets
                .Where(t => t.HorarioId == horarioId)
                .Select(t => new AsientosOcupados
                {
                    Id = t.Butaca.Id_Butacas,
                    Numero = t.Butaca.Numero
                })
                .Distinct()
                .ToListAsync();

        }
        public async Task<Horarios> GetByIdLiteAsync(int id)
        {
            return await _context.Horarios
                .FirstOrDefaultAsync(h => h.Id_Horarios == id);
        }
    }
}
