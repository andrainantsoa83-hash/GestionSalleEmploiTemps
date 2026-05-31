using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestionSalleEmploiTemps.Controllers
{
    [Authorize]
    public class ProfesseursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfesseursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Professeurs
        public async Task<IActionResult> Index()
        {
            var professeurs = await _context.Professeurs.ToListAsync();
            
            // Récupérer le dernier cours pour chaque professeur
            var lastCourses = await _context.Cours
                .GroupBy(c => c.ProfesseurId)
                .Select(g => new { 
                    ProfId = g.Key, 
                    LastDate = g.Max(c => c.HeureFin),
                    Matiere = g.OrderByDescending(c => c.HeureFin).FirstOrDefault().Matiere
                })
                .ToDictionaryAsync(x => x.ProfId, x => new { x.LastDate, x.Matiere });

            ViewBag.LastCourses = lastCourses;
            return View(professeurs);
        }

        // GET: Professeurs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Professeurs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email,Telephone,MotDePasse")] Professeur professeur)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professeur);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(professeur);
        }

        // GET: Professeurs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var professeur = await _context.Professeurs.FindAsync(id);
            if (professeur == null) return NotFound();
            
            return View(professeur);
        }

        // POST: Professeurs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Email,Telephone,MotDePasse")] Professeur professeur)
        {
            if (id != professeur.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professeur);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfesseurExists(professeur.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(professeur);
        }

        // GET: Professeurs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var professeur = await _context.Professeurs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (professeur == null) return NotFound();

            return View(professeur);
        }

        // POST: Professeurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professeur = await _context.Professeurs.FindAsync(id);
            if (professeur != null)
            {
                _context.Professeurs.Remove(professeur);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfesseurExists(int id)
        {
            return _context.Professeurs.Any(e => e.Id == id);
        }
    }
}
