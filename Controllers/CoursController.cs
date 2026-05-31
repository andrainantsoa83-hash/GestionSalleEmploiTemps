using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GestionSalleEmploiTemps.Controllers
{
    [Authorize]
    public class CoursController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly int[] PlanningHours = { 7, 8, 10, 12, 14, 16 };

        public CoursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cours
        public async Task<IActionResult> Index(int? niveauId, int? profFilterId)
        {
            var isProf = User.IsInRole("Professeur");
            var currentProfId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var coursQuery = _context.Cours.AsQueryable();
            
            if (isProf)
            {
                coursQuery = coursQuery.Where(c => c.ProfesseurId == currentProfId);
            }
            else if (profFilterId.HasValue)
            {
                coursQuery = coursQuery.Where(c => c.ProfesseurId == profFilterId.Value);
            }

            if (niveauId.HasValue)
            {
                coursQuery = coursQuery.Where(c => c.NiveauId == niveauId.Value);
            }

            ViewBag.Niveaux = await _context.Niveaux.ToListAsync();
            ViewBag.SelectedNiveauId = niveauId;
            ViewBag.SelectedProfFilterId = profFilterId;
            
            ViewBag.Professeurs = await _context.Professeurs.ToDictionaryAsync(p => p.Id, p => $"{p.Nom} {p.Prenom}");
            ViewBag.Salles = await _context.Salles.ToDictionaryAsync(s => s.Id, s => s.Nom);
            ViewBag.PlanningHours = PlanningHours;

            // Calculer la disponibilité des salles pour chaque créneau
            var allSalles = await _context.Salles.ToListAsync();
            var allCoursGlobal = await _context.Cours.ToListAsync();
            var roomAvailability = new Dictionary<string, List<string>>();

            for (int hourIndex = 0; hourIndex < PlanningHours.Length; hourIndex++)
            {
                var sHr = PlanningHours[hourIndex];
                var nextHr = hourIndex + 1 < PlanningHours.Length ? PlanningHours[hourIndex + 1] : sHr + 2;

                for (int d = 1; d <= 5; d++)
                {
                    var slotStart = TimeSpan.FromHours(sHr);
                    var slotEnd = TimeSpan.FromHours(nextHr);
                    var dayOfWeek = (DayOfWeek)d;

                    var occupiedInSlot = allCoursGlobal
                        .Where(c => c.HeureDebut.DayOfWeek == dayOfWeek &&
                                    slotStart < c.HeureFin.TimeOfDay &&
                                    slotEnd > c.HeureDebut.TimeOfDay)
                        .Select(c => c.SalleId)
                        .ToList();

                    var freeSallesInSlot = allSalles
                        .Where(s => !occupiedInSlot.Contains(s.Id))
                        .Select(s => s.Nom)
                        .ToList();

                    roomAvailability[$"{d}-{sHr}"] = freeSallesInSlot;
                }
            }

            ViewBag.RoomAvailability = roomAvailability;
            
            return View(await coursQuery.ToListAsync());
        }

        // GET: Cours/Create
        public async Task<IActionResult> Create(int? day, int? hour, int? endHour)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentProfId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var cours = new Cours();
            if (!isAdmin) cours.ProfesseurId = currentProfId;

            if (day.HasValue && hour.HasValue)
            {
                // Trouver la date la plus proche correspondant au jour et à l'heure
                DateTime targetDate = DateTime.Today;
                while ((int)targetDate.DayOfWeek != day.Value)
                {
                    targetDate = targetDate.AddDays(1);
                }
                cours.HeureDebut = targetDate.AddHours(hour.Value);
                cours.HeureFin = targetDate.AddHours(endHour ?? hour.Value + 2);
            }
            else 
            {
                cours.HeureDebut = DateTime.Now;
                cours.HeureFin = DateTime.Now.AddHours(2);
            }

            await LoadFormSelectLists(cours);
            return View(cours);
        }

        // POST: Cours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Matiere,HeureDebut,HeureFin,ProfesseurId,SalleId,NiveauId")] Cours cours)
        {
            var isAdmin = User.IsInRole("Admin");
            var currentProfId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!isAdmin)
            {
                cours.ProfesseurId = currentProfId;
            }

            if (ModelState.IsValid)
            {
                ValidateCoursSchedule(cours);

                if (ModelState.IsValid && await HasSalleConflict(cours))
                {
                    ModelState.AddModelError("", "Conflit détecté : La salle est déjà occupée sur ce créneau.");
                }
                else if (ModelState.IsValid && await HasProfesseurConflict(cours))
                {
                    ModelState.AddModelError("", "Conflit detecte : Le professeur a deja un cours sur ce creneau.");
                }
                else if (ModelState.IsValid)
                {
                    _context.Add(cours);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            await LoadFormSelectLists(cours);
            return View(cours);
        }

        // GET: Cours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cours = await _context.Cours.FindAsync(id);
            if (cours == null) return NotFound();

            await LoadFormSelectLists(cours, cours.Id);
            return View(cours);
        }

        // POST: Cours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Matiere,HeureDebut,HeureFin,ProfesseurId,SalleId,NiveauId")] Cours cours)
        {
            if (id != cours.Id) return NotFound();

            var isAdmin = User.IsInRole("Admin");
            var currentProfId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!isAdmin) cours.ProfesseurId = currentProfId;

            if (ModelState.IsValid)
            {
                try
                {
                    ValidateCoursSchedule(cours);

                    if (ModelState.IsValid && await HasSalleConflict(cours, cours.Id))
                    {
                        ModelState.AddModelError("", "Conflit détecté : La salle est déjà occupée sur ce créneau.");
                    }
                    else if (ModelState.IsValid && await HasProfesseurConflict(cours, cours.Id))
                    {
                        ModelState.AddModelError("", "Conflit detecte : Le professeur a deja un cours sur ce creneau.");
                    }
                    else if (ModelState.IsValid)
                    {
                        _context.Update(cours);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoursExists(cours.Id)) return NotFound();
                    else throw;
                }
            }
            await LoadFormSelectLists(cours, cours.Id);
            return View(cours);
        }

        // GET: Cours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cours = await _context.Cours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cours == null)
            {
                return NotFound();
            }

            ViewBag.Professeur = await _context.Professeurs.FirstOrDefaultAsync(p => p.Id == cours.ProfesseurId);
            ViewBag.Salle = await _context.Salles.FirstOrDefaultAsync(s => s.Id == cours.SalleId);

            return View(cours);
        }

        // POST: Cours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours != null)
            {
                _context.Cours.Remove(cours);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoursExists(int id)
        {
            return _context.Cours.Any(e => e.Id == id);
        }

        private void ValidateCoursSchedule(Cours cours)
        {
            if (cours.HeureFin <= cours.HeureDebut)
            {
                ModelState.AddModelError("", "L'heure de fin doit etre apres l'heure de debut.");
            }

            if (cours.HeureDebut.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                ModelState.AddModelError("", "Les cours doivent etre planifies du lundi au vendredi.");
            }
        }

        private async Task<bool> HasSalleConflict(Cours cours, int? excludedCoursId = null)
        {
            return await _context.Cours.AnyAsync(c =>
                (!excludedCoursId.HasValue || c.Id != excludedCoursId.Value) &&
                c.SalleId == cours.SalleId &&
                cours.HeureDebut < c.HeureFin &&
                cours.HeureFin > c.HeureDebut);
        }

        private async Task<bool> HasProfesseurConflict(Cours cours, int? excludedCoursId = null)
        {
            return await _context.Cours.AnyAsync(c =>
                (!excludedCoursId.HasValue || c.Id != excludedCoursId.Value) &&
                c.ProfesseurId == cours.ProfesseurId &&
                cours.HeureDebut < c.HeureFin &&
                cours.HeureFin > c.HeureDebut);
        }

        private async Task LoadFormSelectLists(Cours cours, int? excludedCoursId = null)
        {
            var occupiedSalleIds = await _context.Cours
                .Where(c => (!excludedCoursId.HasValue || c.Id != excludedCoursId.Value) &&
                            cours.HeureDebut < c.HeureFin &&
                            cours.HeureFin > c.HeureDebut)
                .Select(c => c.SalleId)
                .ToListAsync();

            var sallesDisponibles = await _context.Salles
                .Where(s => !occupiedSalleIds.Contains(s.Id) || s.Id == cours.SalleId)
                .ToListAsync();

            ViewData["ProfesseurId"] = new SelectList(_context.Professeurs, "Id", "Nom", cours.ProfesseurId);
            ViewData["SalleId"] = new SelectList(sallesDisponibles, "Id", "Nom", cours.SalleId);
            ViewData["NiveauId"] = new SelectList(_context.Niveaux, "Id", "Nom", cours.NiveauId);
        }
    }
}
