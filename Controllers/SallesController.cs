using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestionSalleEmploiTemps.Controllers
{
    [Authorize]
    public class SallesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SallesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var salles = await _context.Salles.ToListAsync();
            
            // Trouver les salles occupées à l'instant T
            var now = DateTime.Now;
            var occupiedSalleIds = await _context.Cours
                .Where(c => now >= c.HeureDebut && now <= c.HeureFin)
                .Select(c => c.SalleId)
                .ToListAsync();

            ViewBag.OccupiedSalleIds = occupiedSalleIds;
            return View(salles);
        }

        public IActionResult Create()
        {
            ViewData["NiveauId"] = new SelectList(_context.Niveaux, "Id", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Capacite,EstDisponible,Batiment,TypeSalle,NiveauId")] Salle salle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NiveauId"] = new SelectList(_context.Niveaux, "Id", "Nom", salle.NiveauId);
            return View(salle);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var salle = await _context.Salles.FindAsync(id);
            if (salle == null) return NotFound();
            
            ViewData["NiveauId"] = new SelectList(_context.Niveaux, "Id", "Nom", salle.NiveauId);
            return View(salle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Capacite,EstDisponible,Batiment,TypeSalle,NiveauId")] Salle salle)
        {
            if (id != salle.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalleExists(salle.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["NiveauId"] = new SelectList(_context.Niveaux, "Id", "Nom", salle.NiveauId);
            return View(salle);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var salle = await _context.Salles.FirstOrDefaultAsync(m => m.Id == id);
            if (salle == null) return NotFound();
            return View(salle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salle = await _context.Salles.FindAsync(id);
            if (salle != null) _context.Salles.Remove(salle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SalleExists(int id)
        {
            return _context.Salles.Any(e => e.Id == id);
        }
    }
}
