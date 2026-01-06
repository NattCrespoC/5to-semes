using ASP_MVC_Prueba.Models;
using ASP_MVC_Prueba.Entidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using ASP_MVC_Prueba.Services.Service;

namespace ASP_MVC_Prueba.Controllers
{
    public class PeliculaController : Controller
    {
        private CineCenterContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private PeliculasService _peliculasService;
        
        public PeliculaController(CineCenterContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _peliculasService = new PeliculasService(_context);
        }
        [HttpGet]
        public IActionResult Pelis()
        {
            var peliculas = _peliculasService.Pelis().Result;
            return Json(peliculas);
        }
        [HttpGet]
        public IActionResult Genero(string genero)
        {
            if(genero == "estreno")
            {
                return Json(_context.Peliculas.Where(p => p.Estreno == true));
            }
            var peliculas = _context.Peliculas
                  .Where(p => p.Genero != null && p.Genero.ToLower() == genero.ToLower())
                  .ToList();
            return Json(peliculas); // Ensure a return statement is present for all code paths
        }
        [HttpGet]
        public IActionResult Detalles(int id)
        {
            var pelicula = _context.Peliculas
                .Include(p => p.Horarios)
                .FirstOrDefault(p => p.Id_Peliculas == id);
                
            if (pelicula == null)
            {
                return NotFound();
            }
            
            return View(pelicula);
        }
        
        [HttpGet]
        public IActionResult Cartelera()
        {
            return PartialView("~/Views/Home/componentes/cartelera.cshtml");
        }

        ///////////////////////////////////////////
        ///     ADMINISTRACION DE PELICULAS     ///
        ///////////////////////////////////////////
        [HttpGet]
        [Authorize (Roles = "Admin")]
        public IActionResult AdminPelis()
        {
            var peliculas = _context.Peliculas.ToList();
            return View(peliculas);
        }
        [HttpGet]
        [Authorize (Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Peliculas pelicula, IFormFile imagenFile)
        {
            try
            {
                if (pelicula.Estado == null)
                    pelicula.Estado = true;// Default to true if not specified
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imagenFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        // Use the CreatePelicula entity to process the image
                        var createPelicula = new CreatePelicula
                        {
                            ImageBase64 = base64String,
                            Id = pelicula.Id_Peliculas // Generate a unique ID for the image
                        };

                        // Save the image and get the path
                        pelicula.Imagen = createPelicula.guardarImagen();
                    }
                }

                _context.Peliculas.Add(pelicula);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Película creada exitosamente";
                return RedirectToAction("AdminPelis");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar la película: {ex.Message}");
                return View(pelicula);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int id)
        {
            var peliculaEdit = await _peliculasService.GetByIdAsync(id);
            if (peliculaEdit == null)
            {
                return NotFound();
            }
            return View(peliculaEdit);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(Peliculas pelicula, IFormFile imagenFile)
        {
            try
            {
                // Get the existing movie from the database to check for the old image
                var existingPelicula = await _context.Peliculas.FindAsync(pelicula.Id_Peliculas);
                if (existingPelicula == null)
                {
                    return NotFound();
                }

                // Handle image upload if a new image is provided
                if (imagenFile != null && imagenFile.Length > 0)
                {
                    // Delete the old image file if it exists
                    if (!string.IsNullOrEmpty(existingPelicula.Imagen))
                    {
                        // Extract filename from the path
                        string fileName = Path.GetFileName(existingPelicula.Imagen);
                        // Construct the full path to the image in wwwroot/img/peliculas
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "peliculas", fileName);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Process and save the new image
                    using (var memoryStream = new MemoryStream())
                    {
                        await imagenFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        // Use the CreatePelicula entity to process the image
                        var createPelicula = new CreatePelicula
                        {
                            ImageBase64 = base64String,
                            Id = pelicula.Id_Peliculas
                        };

                        // Save the image and update the path
                        pelicula.Imagen = createPelicula.guardarImagen();
                    }
                }
                else
                {
                    // Keep the existing image if no new image is uploaded
                    pelicula.Imagen = existingPelicula.Imagen;
                }
                // Update the movie using the service
                await _peliculasService.UpdateAsync(pelicula);

                TempData["SuccessMessage"] = "Película actualizada exitosamente";
                return RedirectToAction("AdminPelis");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar la película: {ex.Message}");
                return View(pelicula);
            }
        }
        [HttpGet]
        [Authorize (Roles = "Admin")]
        public IActionResult Detalle(int? id)
        {
            var pelicula = _context.Peliculas
                .FirstOrDefault(m => m.Id_Peliculas == id);
            if (pelicula == null)
            {
                return NotFound();
            }
            return View(pelicula);
        }
        [HttpGet]
        [Authorize (Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var pelicula = _context.Peliculas.Find(id);
            if (pelicula == null)
            {
                return NotFound();
            }
            _context.Peliculas.Remove(pelicula);
            _context.SaveChanges();
            return RedirectToAction("AdminPelis");
        }
    }
}
