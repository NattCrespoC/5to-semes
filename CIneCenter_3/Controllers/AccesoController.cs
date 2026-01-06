using ASP_MVC_Prueba.Entidad;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using ASP_MVC_Prueba.Filters;

namespace ASP_MVC_Prueba.Controllers
{
    public class AccesoController : Controller
    {
        private CineCenterContext _context;
        public AccesoController(CineCenterContext context)
        {
            _context = context;
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUser user, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var usuarioExistente = _context.Usuarios
                .FirstOrDefault(u => u.Email == user.Email);

            if (usuarioExistente == null)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico no está registrado.");
                return View(user);
            }
            
            if (usuarioExistente.PasswordHash != user.PasswordHash)
            {
                ModelState.AddModelError(string.Empty, "La contraseña es incorrecta.");
                return View(user);
            }

            // Determinar el rol según si es admin o no
            string rol = usuarioExistente.EsAdmin == true ? "Admin" : "Cliente";
            var claims = new List<Claim> {
                // Añadir el ID del usuario como NameIdentifier - CRUCIAL para el proceso de tickets
                new Claim(ClaimTypes.NameIdentifier, usuarioExistente.Id_Usuarios.ToString()),
                new Claim(ClaimTypes.Name, usuarioExistente.NombreCompleto),
                new Claim(ClaimTypes.Email, usuarioExistente.Email),
                new Claim(ClaimTypes.Role, rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Si es administrador, redirigir a la página de administración
            if (rol == "Admin")
            {
                return RedirectToAction("AdminPelis", "Pelicula");
            }

            // Redirect to returnUrl if provided and it's local URL
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
                
            return RedirectToAction("Index", "Home");
        }

        // GET: Acceso/Registro
        public IActionResult Registro()
        {
            return View();
        }
        
        // POST: Acceso/Registro
        [HttpPost]
        public async Task<IActionResult> Registro(CreateUser model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar si el correo ya está registrado
            var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.Email == model.Email);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya está registrado.");
                return View(model);
            }

            // Crear nuevo usuario
            var nuevoUsuario = new Usuarios
            {
                NombreCompleto = model.NombreCompleto,
                Email = model.Email,
                PasswordHash = model.Password, // En una aplicación real, hashear esta contraseña
                EsAdmin = false, // Por defecto, los usuarios registrados no son administradores
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Iniciar sesión automáticamente
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, nuevoUsuario.NombreCompleto),
                new Claim(ClaimTypes.Email, nuevoUsuario.Email),
                new Claim(ClaimTypes.Role, "Cliente")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["SuccessMessage"] = "¡Registro exitoso! Bienvenido a Cine Center.";
            return RedirectToAction("Index", "Home");
        }
        
        // AJAX Registro
        [HttpPost]
        public async Task<IActionResult> RegistroAjax( CreateUser model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            // Verificar si el correo ya está registrado
            var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.Email == model.Email);
            if (usuarioExistente != null)
            {
                return Json(new { success = false, errors = new[] { "Este correo electrónico ya está registrado." } });
            }

            // Crear nuevo usuario
            var nuevoUsuario = new Usuarios
            {
                NombreCompleto = model.NombreCompleto,
                Email = model.Email,
                PasswordHash = model.Password, // En una aplicación real, hashear esta contraseña
                EsAdmin = false,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Iniciar sesión automáticamente
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, nuevoUsuario.Id_Usuarios.ToString()),
                new Claim(ClaimTypes.Name, nuevoUsuario.NombreCompleto),
                new Claim(ClaimTypes.Email, nuevoUsuario.Email),
                new Claim(ClaimTypes.Role, "Cliente")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Json(new { success = true, message = "Registro exitoso" });
        }
        
        // AJAX Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAjax(LoginUser user, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.Email == user.Email);

            if (usuarioExistente == null)
            {
                return Json(new { success = false, errors = new[] { "El correo electrónico no está registrado." } });
            }
            
            if (usuarioExistente.PasswordHash != user.PasswordHash)
            {
                return Json(new { success = false, errors = new[] { "La contraseña es incorrecta." } });
            }

            string rol = usuarioExistente.EsAdmin == true ? "Admin" : "Cliente";
            var claims = new List<Claim> {
                // Añadir el ID del usuario como NameIdentifier - CRUCIAL para el proceso de tickets
                new Claim(ClaimTypes.NameIdentifier, usuarioExistente.Id_Usuarios.ToString()),
                new Claim(ClaimTypes.Name, usuarioExistente.NombreCompleto),
                new Claim(ClaimTypes.Email, usuarioExistente.Email),
                new Claim(ClaimTypes.Role, rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            string redirectUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Home");
            // si es admin, redirigir a la página de administración
            if (rol == "Admin")
            {
                redirectUrl = Url.Action("AdminPelis", "Pelicula");
            }

            return Json(new { success = true, redirectUrl });
        }

        public async Task<IActionResult> Salir()
        {
            // Sign out from the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Add a success message to TempData to display on the home page
            TempData["SuccessMessage"] = "Has cerrado sesión correctamente.";
            
            // Redirect to the home page after logout
            return RedirectToAction("Index", "Home");
        }
    }
}
