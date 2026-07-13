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
    public class ExamensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamensController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Examens
        public async Task<IActionResult> Index(int? coursId)
        {
            var isAdmin = User.IsInRole("Admin");
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);

            var examensQuery = _context.Examens
                .Include(e => e.Cours)
                .ThenInclude(c => c.Professeur)
                .AsQueryable();

            // Si pas admin, voir seulement les examens de ses cours
            if (!isAdmin)
            {
                examensQuery = examensQuery.Where(e => e.Cours.ProfesseurId == currentProfId);
            }

            if (coursId.HasValue)
            {
                examensQuery = examensQuery.Where(e => e.CoursId == coursId.Value);
            }

            var examens = await examensQuery.ToListAsync();
            
            ViewBag.CoursList = await _context.Cours
                .Include(c => c.Professeur)
                .Select(c => new { c.Id, Display = c.Matiere + " - " + c.Professeur.Nom + " " + c.Professeur.Prenom })
                .ToDictionaryAsync(c => c.Id, c => c.Display);

            return View(examens);
        }

        // GET: Examens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var examen = await _context.Examens
                .Include(e => e.Cours)
                .ThenInclude(c => c.Professeur)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (examen == null) return NotFound();

            return View(examen);
        }

        // GET: Examens/Create
        public async Task<IActionResult> Create(int? coursId)
        {
            if (coursId == null) return NotFound();

            var cours = await _context.Cours.FindAsync(coursId);
            if (cours == null) return NotFound();

            // Vérifier que le cours est terminé (programme fini)
            if (cours.Statut != StatutCours.Termine)
            {
                ModelState.AddModelError("", "Le cours doit être marqué comme 'Terminé' avant de planifier un examen.");
            }

            ViewBag.CoursId = coursId;
            ViewBag.CoursInfo = $"{cours.Matiere} - {cours.HeureDebut:dd/MM/yyyy}";

            return View();
        }

        // POST: Examens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CoursId,DateExamen,Commentaire")] Examen examen)
        {
            var cours = await _context.Cours.FindAsync(examen.CoursId);
            if (cours == null) return NotFound();

            if (cours.Statut != StatutCours.Termine)
            {
                ModelState.AddModelError("", "Le cours doit être marqué comme 'Terminé' avant de planifier un examen.");
            }

            if (examen.DateExamen < cours.HeureFin)
            {
                ModelState.AddModelError("DateExamen", "La date d'examen doit être après la fin du cours.");
            }

            if (ModelState.IsValid)
            {
                examen.EstReussi = false; // Par défaut, pas encore réussi
                _context.Add(examen);
                
                // Changer le statut du cours à "Examen"
                cours.Statut = StatutCours.Examen;
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CoursId = examen.CoursId;
            ViewBag.CoursInfo = $"{cours.Matiere} - {cours.HeureDebut:dd/MM/yyyy}";
            return View(examen);
        }

        // GET: Examens/Validate/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Validate(int? id)
        {
            if (id == null) return NotFound();

            var examen = await _context.Examens
                .Include(e => e.Cours)
                .ThenInclude(c => c.Professeur)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (examen == null) return NotFound();

            return View(examen);
        }

        // POST: Examens/Validate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Validate(int id, bool estReussi, string commentaire)
        {
            var examen = await _context.Examens
                .Include(e => e.Cours)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (examen == null) return NotFound();

            examen.EstReussi = estReussi;
            examen.Commentaire = commentaire;
            examen.DateValidation = DateTime.Now;

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentProfId);
            examen.ValidateurId = currentProfId;

            if (estReussi)
            {
                // Cours complètement terminé, libérer le créneau
                examen.Cours.Statut = StatutCours.Complete;
                examen.Cours.DateCompletion = DateTime.Now;
            }
            else
            {
                // Examen raté, le cours reste en statut Examen pour un nouvel essai
                examen.Cours.Statut = StatutCours.Examen;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Examens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var examen = await _context.Examens
                .Include(e => e.Cours)
                .ThenInclude(c => c.Professeur)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (examen == null) return NotFound();

            return View(examen);
        }

        // POST: Examens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examen = await _context.Examens.FindAsync(id);
            if (examen != null)
            {
                // Remettre le cours en statut Terminé
                var cours = await _context.Cours.FindAsync(examen.CoursId);
                if (cours != null)
                {
                    cours.Statut = StatutCours.Termine;
                }
                
                _context.Examens.Remove(examen);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamenExists(int id)
        {
            return _context.Examens.Any(e => e.Id == id);
        }
    }
}
