using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Service
{
    public class PeliculasService : IPeliculasService
    {
        private readonly CineCenterContext _context;

        public PeliculasService(CineCenterContext context)
        {
            _context = context;
        }
        //Peliculas con estado true
        public async Task<IEnumerable<Peliculas>> Pelis()
        {
            return await _context.Peliculas.Where(p => p.Estado == true).ToListAsync();
        }

        public async Task<IEnumerable<Peliculas>> GetAllAsync()
        {
            return await _context.Peliculas.ToListAsync();
        }

        public async Task<Peliculas> GetByIdAsync(int id)
        {
            return await _context.Peliculas.FindAsync(id);
        }

        public async Task<IEnumerable<Peliculas>> GetByGeneroAsync(string genero)
        {
            return await _context.Peliculas.Where(p => p.Genero == genero).ToListAsync();
        }

        public async Task<IEnumerable<Peliculas>> GetEstrenosAsync()
        {
            return await _context.Peliculas.Where(p => p.Estreno == true).ToListAsync();
        }

        public async Task<Peliculas> CreateAsync(Peliculas pelicula)
        {
            _context.Peliculas.Add(pelicula);
            await _context.SaveChangesAsync();
            return pelicula;
        }

        public async Task<Peliculas> UpdateAsync(Peliculas pelicula)
        {
            var existingPelicula = await _context.Peliculas.FindAsync(pelicula.Id_Peliculas);
            if (existingPelicula == null)
                return null;

            _context.Entry(existingPelicula).CurrentValues.SetValues(pelicula);
            await _context.SaveChangesAsync();
            
            return existingPelicula;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);
            if (pelicula == null)
                return false;

            _context.Peliculas.Remove(pelicula);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
