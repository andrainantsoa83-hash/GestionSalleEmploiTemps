using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestionSalleEmploiTemps.Controllers
{
    [Authorize]
    public class NiveauxController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NiveauxController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            int pageSize = 5;

            var query = _context.Niveaux.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.Nom.Contains(searchString) || n.Description.Contains(searchString));
            }

            int count = await query.CountAsync();
            var niveaux = await query
                .OrderBy(n => n.Nom)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new NiveauxIndexViewModel
            {
                Niveaux = niveaux,
                PageIndex = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                SearchString = searchString
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Description")] Niveau niveau)
        {
            if (ModelState.IsValid)
            {
                _context.Add(niveau);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(niveau);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var niveau = await _context.Niveaux.FindAsync(id);
            if (niveau == null) return NotFound();
            
            return View(niveau);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Description")] Niveau niveau)
        {
            if (id != niveau.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(niveau);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NiveauExists(niveau.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(niveau);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var niveau = await _context.Niveaux
                .FirstOrDefaultAsync(m => m.Id == id);
            if (niveau == null) return NotFound();

            return View(niveau);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var niveau = await _context.Niveaux.FindAsync(id);
            if (niveau != null)
            {
                _context.Niveaux.Remove(niveau);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NiveauExists(int id)
        {
            return _context.Niveaux.Any(e => e.Id == id);
        }
    }
}

