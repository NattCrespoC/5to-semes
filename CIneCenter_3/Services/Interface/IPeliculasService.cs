using ASP_MVC_Prueba.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Interface
{
    public interface IPeliculasService
    {
        Task<IEnumerable<Peliculas>> GetAllAsync();
        Task<Peliculas> GetByIdAsync(int id);
        Task<IEnumerable<Peliculas>> GetByGeneroAsync(string genero);
        Task<IEnumerable<Peliculas>> GetEstrenosAsync();
        Task<Peliculas> CreateAsync(Peliculas pelicula);
        Task<Peliculas> UpdateAsync(Peliculas pelicula);
        Task<bool> DeleteAsync(int id);
    }
}
