using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using ASP_MVC_Prueba.Filters;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Acceso/Login";
        option.ExpireTimeSpan= TimeSpan.FromDays(1);
        option.AccessDeniedPath = "/Home/AccessDenied";
    });

builder.Services.AddDbContext<CineCenterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NatalyConnection"))
);

// Registrar el filtro personalizado
builder.Services.AddScoped<AdminRedirectFilter>();

var app = builder.Build();

//Creacion de la base de datos
using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<CineCenterContext>();
   context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();