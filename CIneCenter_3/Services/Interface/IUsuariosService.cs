using ASP_MVC_Prueba.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Interface
{
    public interface IUsuariosService
    {
        Task<IEnumerable<Usuarios>> GetAllAsync();
        Task<Usuarios> GetByIdAsync(int id);
        Task<Usuarios> GetByEmailAsync(string email);
        Task<Usuarios> CreateAsync(Usuarios usuario);
        Task<Usuarios> UpdateAsync(Usuarios usuario);
        Task<bool> DeleteAsync(int id);
        Task<bool> VerifyPasswordAsync(string email, string password);
    }
}
