using ASP_MVC_Prueba.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Interface
{
    public interface IHorariosService
    {
        Task<IEnumerable<Horarios>> GetAllAsync();
        Task<Horarios> GetByIdAsync(int id);
        Task<IEnumerable<Horarios>> GetByPeliculaIdAsync(int peliculaId);
        Task<IEnumerable<Horarios>> GetBySalaIdAsync(int salaId);
        Task<IEnumerable<Horarios>> GetByFechaAsync(DateTime fecha);
        Task<Horarios> CreateAsync(Horarios horario);
        Task<Horarios> UpdateAsync(Horarios horario);
        Task<bool> DeleteAsync(int id);
        Task<Horarios> GetByIdLiteAsync(int id);
    }
}
