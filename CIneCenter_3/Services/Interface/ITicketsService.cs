using ASP_MVC_Prueba.Controllers;
using ASP_MVC_Prueba.Entidad;
using ASP_MVC_Prueba.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Interface
{
    public interface ITicketsService
    {
        Task<IEnumerable<Tickets>> GetAllAsync();
        Task<Tickets> GetByIdAsync(int id);
        Task<IEnumerable<Tickets>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<Tickets>> GetByHorarioIdAsync(int horarioId);
        Task<Tickets> CreateAsync(Tickets ticket);
        Task<Tickets> UpdateAsync(Tickets ticket);
        Task<bool> DeleteAsync(int id);
        Task<bool> ValidateTicketAsync(int id);
        Task<Horarios> GetHorarioConTicketsYButacasAsync(int horarioId);
        Task<Butacas> GetButacaByIdAsync(int idButaca);
        
        // Métodos de estadísticas
        Task<List<PeliculaTicketsViewModel>> GetPeliculasConEstadisticasAsync();
        Task<List<HorarioTicketsViewModel>> GetHorariosConEstadisticasByPeliculaIdAsync(int peliculaId);
        Task<List<Tickets>> GetTicketsDetalladosByHorarioIdAsync(int horarioId);

        // New method for creating multiple tickets from a CreateTickets request
        Task<(bool Success, string ErrorMessage)> CreateTicketsAsync(CreateTickets createTickets, Horarios horario);
    }
}
