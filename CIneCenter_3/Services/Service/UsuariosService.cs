using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASP_MVC_Prueba.Services.Service
{
    public class UsuariosService : IUsuariosService
    {
        private readonly CineCenterContext _context;

        public UsuariosService(CineCenterContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuarios>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuarios> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuarios> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuarios> CreateAsync(Usuarios usuario)
        {
            // No encriptar la contraseña, usar texto plano
            usuario.FechaRegistro = DateTime.Now;
            
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuarios> UpdateAsync(Usuarios usuario)
        {
            var existingUsuario = await _context.Usuarios.FindAsync(usuario.Id_Usuarios);
            if (existingUsuario == null)
                return null;

            _context.Entry(existingUsuario).CurrentValues.SetValues(usuario);
            await _context.SaveChangesAsync();
            
            return usuario;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var usuario = await GetByEmailAsync(email);
            if (usuario == null)
                return false;

            // Comparación directa de contraseñas en texto plano
            return usuario.PasswordHash == password;
        }
    }
}

