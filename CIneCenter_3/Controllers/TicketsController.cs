using ASP_MVC_Prueba.Entidad;
using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Services.Service;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASP_MVC_Prueba.Controllers
{
    public class TicketsController : Controller
    {
        private readonly CineCenterContext _context;
        private readonly HorariosService _horarioService;
        private TicketsService _ticketsService;

        public TicketsController(CineCenterContext context)
        {
            _context = context;
            _horarioService = new HorariosService(context); // Inicializar el servicio de horarios
            _ticketsService = new TicketsService(context);
        }

        private bool VerificarUsuario()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTickets createTickets)
        {
            try
            {
                if (!VerificarUsuario())
                    return Unauthorized(new { message = "Usuario no autenticado" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Solo log y obtención del horario en el controlador
                var horario = await _horarioService.GetByIdLiteAsync(createTickets.HorarioId);
                if (horario == null)
                    return BadRequest(new { message = "Horario no encontrado" });

                // Delegar toda la lógica de creación de tickets al servicio
                var result = await _ticketsService.CreateTicketsAsync(createTickets, horario);
                
                return result.Success 
                    ? Ok(new { success = true, message = "Tickets creados exitosamente.", redirectUrl = "/Home/MisTickets" })
                    : BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error global: {ex.Message}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQrData([FromBody] QrRequestModel request)
        {
            try
            {
                if (!VerificarUsuario())
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Obtener información del horario
                var horario = await _horarioService.GetByIdAsync(request.HorarioId);
                if (horario == null)
                {
                    return NotFound(new { message = "Horario no encontrado" });
                }

                // Generar un identificador único para este pago
                string paymentReference = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

                // Calcular el monto total
                decimal montoTotal = horario.Precio * request.ButacaIds.Count;

                // Obtener información del usuario
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Crear datos para el QR
                var qrData = new QrDataModel
                {
                    PaymentReference = paymentReference,
                    HorarioId = request.HorarioId,
                    UsuarioId = int.Parse(userId),
                    AsientosSeleccionados = string.Join(",", request.AsientosIds),
                    MontoTotal = montoTotal,
                    FechaGeneracion = DateTime.Now,
                    PeliculaTitulo = horario.Pelicula?.Titulo ?? "Película",
                    FechaHoraFuncion = horario.FechaHora,
                    SalaNombre = horario.Sala?.Nombre ?? "Sala"
                };

                // En producción, aquí podrías guardar estos datos en la base de datos
                // para validarlos cuando el usuario complete el pago

                // También podrías generar una imagen QR real aquí, pero por simplicidad
                // solo devolvemos los datos y el frontend generará la visualización

                return Ok(new
                {
                    success = true,
                    qrData = qrData,
                    qrString = $"CineCenter:{paymentReference}|{qrData.UsuarioId}|{qrData.HorarioId}|{qrData.AsientosSeleccionados}|{qrData.MontoTotal}",
                    expiresAt = DateTime.Now.AddMinutes(10)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar QR: {ex.Message}");
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }
        /////////////////////////////////////////
        ///////// ADMIN SECCION ////////////////
        ///////////////////////////////////////
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            // Obtener películas con conteo de tickets vendidos
            var peliculasConTickets = await _context.Peliculas
                .Select(p => new PeliculaTicketsViewModel
                {
                    Pelicula = p,
                    TotalTickets = p.Horarios.SelectMany(h => h.Tickets).Count(),
                    // Calcular el total de ventas sumando (cantidad de tickets por horario * precio del horario)
                    TotalVentas = p.Horarios.Sum(h => h.Tickets.Count() * h.Precio)
                })
                .ToListAsync();
            
            return View(peliculasConTickets);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HorariosPelicula(int id)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.Horarios)
                .FirstOrDefaultAsync(p => p.Id_Peliculas == id);
                
            if (pelicula == null)
            {
                return NotFound();
            }
            
            // Obtener horarios con conteo de tickets
            var horariosConTickets = await _context.Horarios
                .Where(h => h.PeliculaId == id)
                .Select(h => new HorarioTicketsViewModel
                {
                    Horario = h,
                    TotalTickets = h.Tickets.Count(),
                    // Calcular el total de ventas multiplicando la cantidad de tickets por el precio del horario
                    TotalVentas = h.Tickets.Count() * h.Precio
                })
                .ToListAsync();
            
            ViewBag.Pelicula = pelicula;
            return View(horariosConTickets);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TicketsHorario(int id)
        {
            var horario = await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .FirstOrDefaultAsync(h => h.Id_Horarios == id);
                
            if (horario == null)
            {
                return NotFound();
            }
            
            // Obtener tickets de este horario con detalles
            var tickets = await _context.Tickets
                .Include(t => t.Usuario)
                .Include(t => t.Horario)
                .Include(t => t.Butaca)
                .Include(t => t.Pagos)
                .Include(t => t.Facturas)
                .Where(t => t.HorarioId == id)
                .ToListAsync();
            
            ViewBag.Horario = horario;
            return View(tickets);
        }
    }

    // Modelos para el QR
    public class QrRequestModel
    {
        public int HorarioId { get; set; }
        public List<int> ButacaIds { get; set; }
        public List<string> AsientosIds { get; set; }
    }

    public class QrDataModel
    {
        public string PaymentReference { get; set; }
        public int HorarioId { get; set; }
        public int UsuarioId { get; set; }
        public string AsientosSeleccionados { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string PeliculaTitulo { get; set; }
        public DateTime FechaHoraFuncion { get; set; }
        public string SalaNombre { get; set; }
    }

    // View Models para la jerarquía de tickets
    public class PeliculaTicketsViewModel
    {
        public Peliculas Pelicula { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalVentas { get; set; }
    }

    public class HorarioTicketsViewModel
    {
        public Horarios Horario { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalVentas { get; set; }
    }
}
