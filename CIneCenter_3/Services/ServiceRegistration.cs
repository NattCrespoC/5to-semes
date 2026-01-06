using ASP_MVC_Prueba.Services.Interface;
using ASP_MVC_Prueba.Services.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ASP_MVC_Prueba.Services
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            
            // Register services
            services.AddScoped<IUsuariosService, UsuariosService>();
            services.AddScoped<IPeliculasService, PeliculasService>();
            services.AddScoped<IHorariosService, HorariosService>();
            services.AddScoped<ITicketsService, TicketsService>();
            services.AddScoped<IFacturasService, FacturasService>();
            services.AddScoped<ReportesService>();
        }
    }
}
