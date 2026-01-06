using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Authorization;
using ASP_MVC_Prueba.Entidad;

namespace ASP_MVC_Prueba.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly CineCenterContext _context;

        public UsuarioController(CineCenterContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Usuarios usuario)
        {
            try
            {
                // Guardamos la contraseña en texto plano como fue solicitado
                // Nota: En un entorno real se debería usar encriptación
                usuario.FechaRegistro = DateTime.Now;
                
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Usuario creado exitosamente";
                return RedirectToAction("AdminUsuarios");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el usuario: {ex.Message}");
                return View(usuario);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(Usuarios usuario)
        {
            try
            {
                // Get the existing user from the database
                var existingUsuario = await _context.Usuarios.FindAsync(usuario.Id_Usuarios);
                if (existingUsuario == null)
                {
                    return NotFound();
                }

                // Preserve the original registration date
                usuario.FechaRegistro = existingUsuario.FechaRegistro;

                // Update user properties
                _context.Entry(existingUsuario).CurrentValues.SetValues(usuario);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
                return RedirectToAction("AdminUsuarios");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar el usuario: {ex.Message}");
                return View(usuario);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Tickets)
                .FirstOrDefaultAsync(u => u.Id_Usuarios == id);
                
            if (usuario == null)
            {
                return NotFound();
            }
            
            // Verificar si el usuario tiene tickets u otras referencias
            if (usuario.Tickets != null && usuario.Tickets.Any())
            {
                // Redirigir a la página de confirmación si tiene referencias
                return RedirectToAction("ConfirmDelete", new { id = usuario.Id_Usuarios });
            }
            
            // Si no tiene referencias, eliminar directamente
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Usuario eliminado exitosamente";
            return RedirectToAction("AdminUsuarios");
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Tickets)
                    .ThenInclude(t => t.Butaca)
                .Include(u => u.Tickets)
                    .ThenInclude(t => t.Horario)
                        .ThenInclude(h => h.Pelicula)
                .Include(u => u.Tickets)
                    .ThenInclude(t => t.Horario)
                        .ThenInclude(h => h.Sala)
                .FirstOrDefaultAsync(u => u.Id_Usuarios == id);
                
            if (usuario == null)
            {
                return NotFound();
            }
            
            return View(usuario);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Tickets)
                    .ThenInclude(t => t.Pagos)
                .Include(u => u.Tickets)
                    .ThenInclude(t => t.Facturas)
                .FirstOrDefaultAsync(u => u.Id_Usuarios == id);
                
            if (usuario == null)
            {
                return NotFound();
            }
            
            // Eliminar en cascada: primero las entidades relacionadas con los tickets
            if (usuario.Tickets != null && usuario.Tickets.Any())
            {
                foreach (var ticket in usuario.Tickets)
                {
                    // Eliminar pagos asociados al ticket
                    if (ticket.Pagos != null)
                    {
                        _context.Pagos.Remove(ticket.Pagos);
                    }
                    
                    // Eliminar facturas asociadas al ticket
                    if (ticket.Facturas != null)
                    {
                        _context.Facturas.Remove(ticket.Facturas);
                    }
                }
                
                // Luego eliminar los tickets
                _context.Tickets.RemoveRange(usuario.Tickets);
            }
            
            // Finalmente eliminar el usuario
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Usuario y todos sus datos relacionados eliminados exitosamente";
            return RedirectToAction("AdminUsuarios");
        }
    }
}
