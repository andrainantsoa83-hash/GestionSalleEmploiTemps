using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using GestionSalleEmploiTemps.Services;

namespace GestionSalleEmploiTemps.Controllers
{
    [Authorize]
    public class CoursController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IPdfGeneratorService _pdfService;
        private static readonly int[] PlanningHours = { 7, 8, 10, 12, 14, 16 };

        public CoursController(ApplicationDbContext context, IWebHostEnvironment env, IPdfGeneratorService pdfService)
        {
            _context = context;
            _env = env;
            _pdfService = pdfService;
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePdf(int niveauId, int? salleId, string weekDate)
        {
            var niveau = await _context.Niveaux.FindAsync(niveauId);
            if (niveau == null) return NotFound();

            var nomSalle = "Non affectée";
            if (salleId.HasValue)
            {
                var salle = await _context.Salles.FindAsync(salleId.Value);
                nomSalle = salle?.Nom ?? nomSalle;
            }

            DateTime dateDebutSemaine;
            if (!DateTime.TryParse(weekDate, out dateDebutSemaine))
            {
                dateDebutSemaine = GetStartOfWeek(DateTime.Today);
            }
            DateTime dateFinSemaine = dateDebutSemaine.AddDays(7);

            var cours = await _context.Cours
                .Include(c => c.Professeur)
                .Include(c => c.Salle)
                .Where(c => c.NiveauId == niveauId && c.HeureDebut >= dateDebutSemaine && c.HeureDebut < dateFinSemaine)
                .ToListAsync();

            string logoPath = Path.Combine(_env.WebRootPath, "images.jpg");

            var model = new EmploiTempsPdfModel
            {
                NiveauNom = niveau.Nom,
                SemaineText = $"du {dateDebutSemaine:dd/MM/yyyy} au {dateFinSemaine.AddDays(-1):dd/MM/yyyy}",
                LogoPath = logoPath,
                CoursList = cours
            };

            byte[] pdfBytes = _pdfService.GenerateEmploiTempsPdf(model);

            string fileName = $"EmploiTemps_{niveau.Nom.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Cours
        public async Task<IActionResult> Index(int? salleId, int? profFilterId, DateTime? weekDate)
        {
            var isProf = User.IsInRole("Professeur");
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);

            DateTime currentDate = weekDate ?? DateTime.Today;
            int diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime monday = currentDate.AddDays(-1 * diff).Date;
            DateTime saturday = monday.AddDays(5);

            var coursQuery = _context.Cours.Where(c => c.HeureDebut >= monday && c.HeureDebut < saturday);
            
            // On ne filtre plus automatiquement sur le professeur courant :
            // Tous les professeurs peuvent VOIR l'emploi du temps complet de la salle.
            if (profFilterId.HasValue)
            {
                coursQuery = coursQuery.Where(c => c.ProfesseurId == profFilterId.Value);
            }

            if (!salleId.HasValue)
            {
                var firstSalle = await _context.Salles.OrderBy(s => s.Nom).FirstOrDefaultAsync();
                if (firstSalle != null)
                {
                    salleId = firstSalle.Id;
                }
            }

            if (salleId.HasValue)
            {
                coursQuery = coursQuery.Where(c => c.SalleId == salleId.Value);
            }

            ViewBag.SallesDisponibles = await _context.Salles.OrderBy(s => s.Nom).ToListAsync();
            ViewBag.SelectedSalleId = salleId;
            ViewBag.SelectedProfFilterId = profFilterId;
            ViewBag.CurrentWeekDate = monday;
            
            ViewBag.Professeurs = await _context.Professeurs.ToDictionaryAsync(p => p.Id, p => $"{p.Nom} {p.Prenom}");
            ViewBag.Salles = await _context.Salles.ToDictionaryAsync(s => s.Id, s => s.Nom);
            ViewBag.Niveaux = await _context.Niveaux.ToDictionaryAsync(n => n.Id, n => n.Nom);
            ViewBag.PlanningHours = PlanningHours;

            // Calculer la disponibilité des salles pour chaque créneau de cette semaine spécifique
            var allSalles = await _context.Salles.ToListAsync();
            var allCoursGlobal = await _context.Cours.Where(c => c.HeureDebut >= monday && c.HeureDebut < saturday).ToListAsync();
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
                        .Where(s => !occupiedInSlot.Contains(s.Id) && s.Batiment != "A")
                        .Select(s => $"Bâtiment {s.Batiment} - {s.Nom}")
                        .ToList();

                    roomAvailability[$"{d}-{sHr}"] = freeSallesInSlot;
                }
            }

            ViewBag.RoomAvailability = roomAvailability;
            
            return View(await coursQuery.ToListAsync());
        }

        // GET: Cours/Create
        public async Task<IActionResult> Create(int? day, int? hour, int? endHour, DateTime? weekDate, int? salleId)
        {
            var isAdmin = User.IsInRole("Admin");
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);
            
            var cours = new Cours();
            if (!isAdmin) cours.ProfesseurId = currentProfId;

            if (salleId.HasValue)
            {
                cours.SalleId = salleId.Value;
            }

            if (day.HasValue && hour.HasValue)
            {
                DateTime targetDate = weekDate ?? DateTime.Today;
                // Aller au lundi de la semaine
                int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                targetDate = targetDate.AddDays(-1 * diff).Date;
                
                // Ajouter les jours pour arriver au jour demandé (day = 1 pour lundi, 2 pour mardi...)
                targetDate = targetDate.AddDays(day.Value - 1);

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
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);

            if (!isAdmin)
            {
                cours.ProfesseurId = currentProfId;
            }

            ModelState.Remove("Professeur");
            ModelState.Remove("Salle");
            ModelState.Remove("Niveau");

            if (ModelState.IsValid)
            {
                ValidateCoursSchedule(cours);

                if (ModelState.IsValid && await HasSalleConflict(cours))
                {
                    ModelState.AddModelError("", "Conflit détecté : La salle est déjà occupée sur ce créneau.");
                }
                else if (ModelState.IsValid)
                {
                    _context.Add(cours);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { salleId = cours.SalleId });
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

            var isAdmin = User.IsInRole("Admin");
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);
            if (!isAdmin && cours.ProfesseurId != currentProfId) return Unauthorized();

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
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);

            // Securité : vérifier le cours original
            var originalCours = await _context.Cours.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (originalCours == null) return NotFound();
            if (!isAdmin && originalCours.ProfesseurId != currentProfId) return Unauthorized();

            if (!isAdmin) cours.ProfesseurId = currentProfId;

            ModelState.Remove("Professeur");
            ModelState.Remove("Salle");
            ModelState.Remove("Niveau");

            if (ModelState.IsValid)
            {
                try
                {
                    ValidateCoursSchedule(cours);

                    if (ModelState.IsValid && await HasSalleConflict(cours, cours.Id))
                    {
                        ModelState.AddModelError("", "Conflit détecté : La salle est déjà occupée sur ce créneau.");
                    }
                    else if (ModelState.IsValid)
                    {
                        _context.Update(cours);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index), new { salleId = cours.SalleId });
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

            var isAdmin = User.IsInRole("Admin");
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);
            if (!isAdmin && cours.ProfesseurId != currentProfId) return Unauthorized();

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
            int? salleId = null;
            if (cours != null)
            {
                var isAdmin = User.IsInRole("Admin");
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);
                if (!isAdmin && cours.ProfesseurId != currentProfId) return Unauthorized();

                salleId = cours.SalleId;
                _context.Cours.Remove(cours);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { salleId = salleId });
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
                .Where(s => s.Batiment != "A" && (!occupiedSalleIds.Contains(s.Id) || s.Id == cours.SalleId))
                .OrderByDescending(s => s.NiveauId == cours.NiveauId) // La salle dédiée en premier
                .ThenBy(s => s.Batiment).ThenBy(s => s.Nom)
                .ToListAsync();

            ViewData["ProfesseurId"] = new SelectList(_context.Professeurs, "Id", "Nom", cours.ProfesseurId);
            ViewData["SalleId"] = new SelectList(sallesDisponibles, "Id", "Nom", cours.SalleId);
            ViewData["NiveauId"] = new SelectList(_context.Niveaux, "Id", "Nom", cours.NiveauId);
        }

        // GET: Cours/GetSallesDisponibles
        [HttpGet]
        public async Task<IActionResult> GetSallesDisponibles(DateTime debut, DateTime fin, int? excludedCoursId, int? niveauId)
        {
            var occupiedSalleIds = await _context.Cours
                .Where(c => (!excludedCoursId.HasValue || c.Id != excludedCoursId.Value) &&
                            debut < c.HeureFin &&
                            fin > c.HeureDebut)
                .Select(c => c.SalleId)
                .ToListAsync();

            var sallesDisponiblesQuery = _context.Salles
                .Where(s => s.Batiment != "A" && !occupiedSalleIds.Contains(s.Id));

            // On ne filtre pas strictement, on met juste la salle du niveau en premier
            var sallesDisponibles = await sallesDisponiblesQuery
                .OrderByDescending(s => niveauId.HasValue && s.NiveauId == niveauId.Value)
                .ThenBy(s => s.Batiment).ThenBy(s => s.Nom)
                .Select(s => new { id = s.Id, nom = $"Bâtiment {s.Batiment} - {s.Nom}" + (niveauId.HasValue && s.NiveauId == niveauId.Value ? " (Salle Préférée)" : "") })
                .ToListAsync();

            return Json(sallesDisponibles);
        }
    }
}
