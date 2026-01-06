using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASP_MVC_Prueba.Models;
using Microsoft.AspNetCore.Authorization;

namespace ASP_MVC_Prueba.Controllers
{
    public class HorariosController : Controller
    {
        private readonly CineCenterContext _context;

        public HorariosController(CineCenterContext context)
        {
            _context = context;
        }

        // GET: Horarios
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            // Limit week offset to reasonable values (0 for current week, 1 for next week)
            weekOffset = Math.Clamp(weekOffset, 0, 1);
            
            // Get current date info
            DateTime today = DateTime.Today;
            
            // Find the Thursday of the current cinema week
            // If today is Thursday through Wednesday, it's in the current week
            // Otherwise, it's in the previous week
            DayOfWeek currentDayOfWeek = today.DayOfWeek;
            
            // Calculate days to subtract to get to Thursday
            int daysToThursday = ((int)today.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
            
            // If today is a Thursday, we want this Thursday, not last Thursday
            if (daysToThursday == 0)
            {
                daysToThursday = 0;
            }
            
            // Calculate the Thursday of the current cinema week
            DateTime thursdayOfCurrentWeek = today.AddDays(-daysToThursday);
            
            // Apply week offset to get the display week
            DateTime thursdayOfDisplayWeek = thursdayOfCurrentWeek.AddDays(7 * weekOffset);
            
            // Create list of days for the week (Thursday to Wednesday)
            List<DateTime> currentWeekDays = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                currentWeekDays.Add(thursdayOfDisplayWeek.AddDays(i));
            }
            
            // Calculate start of next week
            DateTime thursdayOfNextWeek = thursdayOfDisplayWeek.AddDays(7);
            
            // Create list of days for the next week
            List<DateTime> nextWeekDays = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                nextWeekDays.Add(thursdayOfNextWeek.AddDays(i));
            }

            // Create list of time slots
            List<TimeSpan> timeSlots = new List<TimeSpan>();
            for (int hour = 10; hour <= 20; hour += 2) // 10:00 to 20:00 in 2-hour increments
            {
                timeSlots.Add(new TimeSpan(hour, 0, 0));
            }

            // Get all salas
            var salas = await _context.Salas.ToListAsync();

            // Get all horarios for both weeks
            var startDate = thursdayOfDisplayWeek.Date;
            var endDate = thursdayOfNextWeek.AddDays(7).Date; // End of next week
            
            var horarios = await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .Where(h => h.FechaHora >= startDate && h.FechaHora < endDate)
                .ToListAsync();

            // Get all películas for dropdown
            var peliculas = await _context.Peliculas.ToListAsync();

            // Set ViewBag data
            ViewBag.CurrentWeekDays = currentWeekDays;
            ViewBag.NextWeekDays = nextWeekDays;
            ViewBag.TimeSlots = timeSlots;
            ViewBag.Salas = salas;
            ViewBag.Peliculas = new SelectList(peliculas, "Id_Peliculas", "Titulo");
            ViewBag.WeekOffset = weekOffset;
            
            return View(horarios);
        }

        // Check if a time slot is available
        private async Task<bool> IsTimeSlotAvailable(int salaId, DateTime dateTime, int peliculaId)
        {
            // Get the película to check its duration
            var pelicula = await _context.Peliculas.FindAsync(peliculaId);
            if (pelicula == null) return false;

            // Parse the duration from string to double
            if (!double.TryParse(pelicula.Duracion, out double duracionMinutos))
            {
                // Handle parsing error - default to 120 minutes if can't parse
                duracionMinutos = 120;
            }

            // Calculate end time based on película duration
            var startTime = dateTime;
            var endTime = startTime.AddMinutes(duracionMinutos);

            // Check for conflicts
            var conflicts = await _context.Horarios
                .Include(h => h.Pelicula)
                .Where(h => h.SalaId == salaId)
                .ToListAsync();

            foreach (var horario in conflicts)
            {
                var existingStartTime = horario.FechaHora;
                
                // Parse the duration from string to double
                if (!double.TryParse(horario.Pelicula.Duracion, out double existingDuracionMinutos))
                {
                    // Handle parsing error - default to 120 minutes if can't parse
                    existingDuracionMinutos = 120;
                }
                
                var existingEndTime = existingStartTime.AddMinutes(existingDuracionMinutos);

                // Check if there's an overlap
                if (startTime < existingEndTime && endTime > existingStartTime)
                {
                    return false; // There's a conflict
                }
            }

            return true; // No conflicts found
        }

        // GET: Horarios/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horarios = await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .FirstOrDefaultAsync(m => m.Id_Horarios == id);
            if (horarios == null)
            {
                return NotFound();
            }

            return View(horarios);
        }

        // GET: Horarios/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? salaId, DateTime? fechaHora)
        {
            ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id_Peliculas", "Titulo");
            ViewData["SalaId"] = new SelectList(_context.Salas, "Id_Salas", "Nombre");
            
            // If we have pre-selected values, set them in the model
            var horario = new Horarios();
            if (salaId.HasValue)
            {
                horario.SalaId = salaId.Value;
            }
            
            if (fechaHora.HasValue)
            {
                // Ensure we're using the exact date passed in the parameter
                horario.FechaHora = fechaHora.Value;
                
                // Log the date for debugging
                System.Diagnostics.Debug.WriteLine($"Received date: {fechaHora.Value.ToString("yyyy-MM-dd HH:mm:ss")}");
                System.Diagnostics.Debug.WriteLine($"Day of week: {fechaHora.Value.DayOfWeek}");
            }
            else
            {
                // Set current date/time as default if no date is provided
                horario.FechaHora = DateTime.Now;
            }
            
            return View(horario);
        }

        // POST: Horarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id_Horarios,SalaId,PeliculaId,FechaHora,Precio")] Horarios horarios, bool aplicarSemanaCompleta = false)
        {
            if (true/*ModelState.IsValid*/)
            {
                if (!aplicarSemanaCompleta)
                {
                    // Single day processing (existing logic)
                    if (await IsTimeSlotAvailable(horarios.SalaId, horarios.FechaHora, horarios.PeliculaId))
                    {
                        _context.Add(horarios);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Horario creado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "El horario seleccionado ya está ocupado o genera un conflicto con otro horario existente.");
                        ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id_Peliculas", "Titulo", horarios.PeliculaId);
                        ViewData["SalaId"] = new SelectList(_context.Salas, "Id_Salas", "Nombre", horarios.SalaId);
                        return View(horarios);
                    }
                }
                else
                {
                    // Weekly processing for the cinema week (Thursday to Wednesday)
                    // Create lists to store days with conflicts and successful days
                    List<string> conflictedDays = new List<string>();
                    List<string> successDays = new List<string>();

                    // Get the selected time
                    TimeSpan selectedTime = horarios.FechaHora.TimeOfDay;
                    
                    // Use the selected date from the form as the reference date
                    DateTime selectedDate = horarios.FechaHora.Date;
                    
                    // Find the Thursday of the cinema week containing the selected date
                    int daysToThursday = ((int)selectedDate.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
                    
                    // Calculate the Thursday of the selected week
                    DateTime thursdayOfWeek = selectedDate.AddDays(-daysToThursday);
                    
                    // For debugging
                    System.Diagnostics.Debug.WriteLine($"Selected date: {selectedDate.ToString("yyyy-MM-dd")}");
                    System.Diagnostics.Debug.WriteLine($"Day of week: {selectedDate.DayOfWeek}");
                    System.Diagnostics.Debug.WriteLine($"Days to Thursday: {daysToThursday}");
                    System.Diagnostics.Debug.WriteLine($"Thursday of week: {thursdayOfWeek.ToString("yyyy-MM-dd")}");
                    
                    // Create a schedule for each day of the cinema week (Thursday to Wednesday)
                    for (int i = 0; i < 7; i++)
                    {
                        DateTime dateForDay = thursdayOfWeek.AddDays(i).Add(selectedTime);
                        string dayName = dateForDay.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
                        
                        // For debugging
                        System.Diagnostics.Debug.WriteLine($"Processing day {i}: {dateForDay.ToString("yyyy-MM-dd")} ({dayName})");
                        
                        // Check if this time slot is available
                        if (await IsTimeSlotAvailable(horarios.SalaId, dateForDay, horarios.PeliculaId))
                        {
                            var newHorario = new Horarios
                            {
                                SalaId = horarios.SalaId,
                                PeliculaId = horarios.PeliculaId,
                                FechaHora = dateForDay,
                                Precio = horarios.Precio
                            };
                            
                            _context.Add(newHorario);
                            successDays.Add($"{dayName} ({dateForDay.ToString("dd/MM")})");
                        }
                        else
                        {
                            conflictedDays.Add($"{dayName} ({dateForDay.ToString("dd/MM")})");
                        }
                    }
                    
                    // Save all the successful horarios
                    await _context.SaveChangesAsync();
                    
                    // Prepare success message
                    if (successDays.Count > 0)
                    {
                        string successMessage = $"Horarios creados exitosamente para: {string.Join(", ", successDays)}";
                        if (conflictedDays.Count > 0)
                        {
                            successMessage += $". Se omitieron los días con conflictos: {string.Join(", ", conflictedDays)}";
                        }
                        TempData["SuccessMessage"] = successMessage;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "No se pudo crear ningún horario debido a conflictos en todos los días.";
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id_Peliculas", "Titulo", horarios.PeliculaId);
            ViewData["SalaId"] = new SelectList(_context.Salas, "Id_Salas", "Nombre", horarios.SalaId);
            return View(horarios);
        }

        // GET: Horarios/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horarios = await _context.Horarios.FindAsync(id);
            if (horarios == null)
            {
                return NotFound();
            }
            ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id_Peliculas", "Titulo", horarios.PeliculaId);
            ViewData["SalaId"] = new SelectList(_context.Salas, "Id_Salas", "Nombre", horarios.SalaId);
            return View(horarios);
        }

        // POST: Horarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id_Horarios,SalaId,PeliculaId,FechaHora,Precio")] Horarios horarios)
        {
            if (id != horarios.Id_Horarios)
            {
                return NotFound();
            }

            if (true/*ModelState.IsValid*/)
            {
                try
                {
                    // Get the existing horario with tracking enabled
                    var existingHorario = await _context.Horarios
                        .Include(h => h.Pelicula)
                        .FirstOrDefaultAsync(h => h.Id_Horarios == id);
                    
                    if (existingHorario == null)
                    {
                        return NotFound();
                    }
                    
                    // Check if we're changing time, sala or película
                    bool isTimeOrSalaChanged = existingHorario.SalaId != horarios.SalaId || 
                                               existingHorario.FechaHora != horarios.FechaHora || 
                                               existingHorario.PeliculaId != horarios.PeliculaId;
                    
                    // If we're changing time, sala or película, check for conflicts
                    if (isTimeOrSalaChanged)
                    {
                        // Temporarily detach the entity to check for conflicts without it
                        _context.Entry(existingHorario).State = EntityState.Detached;
                        
                        // Check for conflicts with other horarios
                        if (!await IsTimeSlotAvailable(horarios.SalaId, horarios.FechaHora, horarios.PeliculaId))
                        {
                            ModelState.AddModelError("", "El horario seleccionado ya está ocupado o genera un conflicto con otro horario existente.");
                            ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id_Peliculas", "Titulo", horarios.PeliculaId);
                            ViewData["SalaId"] = new SelectList(_context.Salas, "Id_Salas", "Nombre", horarios.SalaId);
                            return View(horarios);
                        }
                        
                        // Re-attach and update properties
                        _context.Horarios.Attach(existingHorario);
                        existingHorario.SalaId = horarios.SalaId;
                        existingHorario.PeliculaId = horarios.PeliculaId;
                        existingHorario.FechaHora = horarios.FechaHora;
                        existingHorario.Precio = horarios.Precio;
                    }
                    else
                    {
                        // Just update price
                        existingHorario.Precio = horarios.Precio;
                    }
                    
                    // Mark as modified and save
                    _context.Entry(existingHorario).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HorariosExists(horarios.Id_Horarios))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Horario actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id_Peliculas", "Titulo", horarios.PeliculaId);
            ViewData["SalaId"] = new SelectList(_context.Salas, "Id_Salas", "Nombre", horarios.SalaId);
            return View(horarios);
        }

        // GET: Horarios/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var horarios = await _context.Horarios
                .Include(h => h.Pelicula)
                .Include(h => h.Sala)
                .FirstOrDefaultAsync(m => m.Id_Horarios == id);
            if (horarios == null)
            {
                return NotFound();
            }

            return View(horarios);
        }

        // POST: Horarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var horarios = await _context.Horarios.FindAsync(id);
            if (horarios != null)
            {
                _context.Horarios.Remove(horarios);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Horario eliminado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        private bool HorariosExists(int id)
        {
            return _context.Horarios.Any(e => e.Id_Horarios == id);
        }
    }
}
