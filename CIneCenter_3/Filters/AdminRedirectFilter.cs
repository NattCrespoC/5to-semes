using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASP_MVC_Prueba.Filters
{
    public class AdminRedirectFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            
            // Verificar si el usuario está autenticado y es un administrador
            if (user.Identity.IsAuthenticated && user.IsInRole("Admin"))
            {
                // Verificar si ya está en la página de administración
                var controllerName = context.RouteData.Values["controller"]?.ToString();
                var actionName = context.RouteData.Values["action"]?.ToString();
                
                // Si no está en la página de administración, redirigir
                if (!(controllerName == "Pelicula" && actionName == "AdminPelis"))
                {
                    // Permitir acceder a Acceso/Salir para cerrar sesión
                    if (!(controllerName == "Acceso" && actionName == "Salir"))
                    {
                        context.Result = new RedirectToActionResult("AdminPelis", "Pelicula", null);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No necesitamos hacer nada después de la ejecución
        }
    }
}
