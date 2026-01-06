using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASP_MVC_Prueba.Controllers
{
    public class HorarioController : Controller
    {
        private readonly HorariosService _horariosService;
        
        public HorarioController(CineCenterContext context)
        {
            _horariosService = new HorariosService(context);
        }
        // GET: 
        //Lista de asientos ocupados por tickets de un horario especifico
        public async Task<IActionResult> AsientosOcupados(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            
            var seats = await _horariosService.GetAsientosOcupadosAsync(id);
            
            if (seats == null)
            {
                return NotFound();
            }

            return Json(JsonSerializer.Serialize(seats, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 64
            }));
        }

        public async Task<IActionResult> Index(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var horario = await _horariosService.GetHorarioConTicketsYButacasAsync(id);

            return Ok(JsonSerializer.Serialize(horario, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 64
            }));
        }
        
        // Nuevo método para mostrar la vista detallada del horario con sus asientos
        public async Task<IActionResult> Detalle(int id)
        {
            // Verificar si el ID es válido
            if (id <= 0)
            {
                return NotFound();
            }
            
            // Obtener el horario con sus tickets y butacas
            var horario = await _horariosService.GetHorarioConTicketsYButacasAsync(id);
            
            // Verificar si el horario existe
            if (horario == null)
            {
                return NotFound();
            }
            
            // Verificar si el usuario está autenticado (opcional)
            var usuarioAutenticado = User.Identity.IsAuthenticated;
            if (!usuarioAutenticado)
            {
                // Si no está autenticado, indicar que debe mostrar el modal de login
                ViewData["ShowLoginModal"] = true;
            }
            
            // Retornar la vista con el modelo
            return View("~/Views/Home/HorarioDetalle.cshtml", horario);
        }
    }
}
