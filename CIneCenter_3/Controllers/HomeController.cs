using System.Diagnostics;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using ASP_MVC_Prueba.Services.Service;
using ASP_MVC_Prueba.Filters;

namespace ASP_MVC_Prueba.Controllers
{
    [ServiceFilter(typeof(AdminRedirectFilter))]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CineCenterContext _context;
        private readonly HorariosService _horariosService;
        private readonly TicketsService _ticketsService;
        public HomeController(ILogger<HomeController> logger, CineCenterContext context)
        {
            _logger = logger;
            _context = context;
            _horariosService = new HorariosService(context);
            _ticketsService = new TicketsService(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Modificar para agregar redirecci�n personalizada
        public IActionResult Perfil()
        {
            // Verificar si el usuario est� autenticado
            if (!User.Identity.IsAuthenticated)
            {
                // Redirigir a la p�gina de inicio con mensaje de error
                TempData["ErrorMessage"] = "Debe iniciar sesi�n para acceder a esta p�gina";
                return RedirectToAction("Index");
            }

            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "No se pudo obtener la informaci�n del usuario";
                return RedirectToAction("Index");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == userEmail);
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado";
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        // Agregar un m�todo para acceder directamente a la p�gina de perfil con token
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PerfilConToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Token de autenticaci�n no v�lido";
                return RedirectToAction("Index");
            }

            // Almacenar el token en una cookie para la autenticaci�n
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddMinutes(30)
            });

            return RedirectToAction("Perfil");
        }

        // For unauthorized users trying to access protected areas
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Detalles(int id)
        {
            // Get current date and time
            var ahora = DateTime.Now;
            
            // Buscar la película por ID incluyendo sus horarios
            var pelicula = _context.Peliculas
                .Include(p => p.Horarios.Where(h => h.FechaHora > ahora).OrderBy(h => h.FechaHora))
                .ThenInclude(h => h.Sala)
                .FirstOrDefault(p => p.Id_Peliculas == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            // Retornar la vista con el objeto Peliculas que incluye horarios
            return View(pelicula);
        }

        [HttpGet]
        public async Task<IActionResult> HorarioDetalle(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["HorarioId"] = id;
                var horarioBasico = _context.Horarios
                    .Include(h => h.Pelicula)
                    .Include(h => h.Sala)
                    .FirstOrDefault(h => h.Id_Horarios == id);
                if (horarioBasico == null)
                {
                    return NotFound();
                }
                TempData["PeliculaId"] = horarioBasico.PeliculaId;

                // Redirigir al login con ReturnUrl
                // return RedirectToAction("Login", "Acceso", new { returnUrl = Url.Action("HorarioDetalle", "Home", new { id }) });
                return RedirectToAction("AccessDenied", "Home");
            }
            // Usuario autenticado: Buscar el horario por ID, incluyendo la sala y la película
            var horario = await _horariosService.GetHorarioConTicketsYButacasAsync(id);
            if (horario == null)
            {
                return NotFound();
            }

            // Asegurarnos de que las butacas de la sala estén cargadas
            if (horario.Sala != null && (horario.Sala.Butacas == null || !horario.Sala.Butacas.Any()))
            {
                // Si no hay butacas, podríamos cargarlas explícitamente
                var butacas = await _context.Butacas.Where(b => b.SalaId == horario.SalaId).ToListAsync();
                if (horario.Sala.Butacas == null)
                    horario.Sala.Butacas = new List<Butacas>();

                foreach (var butaca in butacas)
                {
                    horario.Sala.Butacas.Add(butaca);
                }
            }

            // Retornar la vista con el objeto Horarios que incluye sala y película
            return View(horario);
        }

        [HttpGet]
        public async Task<IActionResult> TicketsCompra(int HorarioId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { message = "Usuario no autenticado." });
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var horarioId = _ticketsService.GetHorarioConTicketsYButacasAsync(HorarioId);
            return View();
        }

        [HttpGet]
        [Authorize] // Asegurarnos que este método requiere autenticación
        public async Task<IActionResult> MisTickets(int? id = null)
        {
            // Obtener el ID del usuario de los claims
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return RedirectToAction("Login", "Acceso");
            }

            // Si se proporciona un ID de horario, mostrar solo los tickets de ese horario
            if (id.HasValue)
            {
                // Verificar que el horario existe
                var horario = await _context.Horarios
                    .Include(h => h.Pelicula)
                    .Include(h => h.Sala)
                    .FirstOrDefaultAsync(h => h.Id_Horarios == id);
                    
                if (horario == null)
                {
                    TempData["ErrorMessage"] = "El horario solicitado no existe";
                    return RedirectToAction("Index");
                }
                
                // Obtener los tickets del usuario para ese horario específico
                var tickets = await _context.Tickets
                    .Include(t => t.Usuario)
                    .Include(t => t.Butaca)
                    .Include(t => t.Horario)
                        .ThenInclude(h => h.Pelicula)
                    .Include(t => t.Horario)
                        .ThenInclude(h => h.Sala)
                    .Where(t => t.HorarioId == id && t.UsuarioId == userId)
                    .ToListAsync();
                
                // Si el usuario no tiene tickets para este horario, redirigir a la página de compra
                if (tickets == null || !tickets.Any())
                {
                    TempData["Message"] = "No tienes entradas para esta función. ¿Deseas comprar?";
                    return RedirectToAction("HorarioDetalle", new { id });
                }
                
                // Pasar información adicional a la vista
                ViewBag.Horario = horario;
                
                return View(tickets);
            }
            else
            {
                // Comportamiento original: obtener todos los tickets del usuario
                var tickets = await _ticketsService.GetByUsuarioIdAsync(userId);
                return View(tickets);
            }
        }

        [HttpGet]
        public IActionResult GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }
            
            // Intentar obtener el ID del usuario desde los claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return Json(new { success = true, userId = userId });
            }
            
            return Json(new { success = false, message = "No se pudo determinar el ID del usuario" });
        }
    }
}
