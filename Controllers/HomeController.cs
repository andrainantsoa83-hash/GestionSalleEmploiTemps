using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Models;
using GestionSalleEmploiTemps.Data;

namespace GestionSalleEmploiTemps.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [AllowAnonymous]
    public IActionResult Landing()
    {
        // Si l'utilisateur est déjà connecté, on l'envoie vers le tableau de bord
        if (User.Identity.IsAuthenticated)
            return RedirectToAction("Index");
            
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var profCount = await _context.Professeurs.CountAsync();
        var salleCount = await _context.Salles.CountAsync();
        var coursCount = await _context.Cours.CountAsync();

        var now = DateTime.Now;
        var occupiedSallesNowCount = await _context.Cours
            .Where(c => c.HeureDebut <= now && c.HeureFin > now)
            .Select(c => c.SalleId)
            .Distinct()
            .CountAsync();
        
        ViewBag.ProfCount = profCount;
        ViewBag.SalleCount = salleCount;
        ViewBag.CoursCount = coursCount;
        ViewBag.SallesDisponiblesActuellement = salleCount - occupiedSallesNowCount;

        // Taux d'occupation global (approximation: 5 jours * 4 slots * nbSalles)
        int totalPossibleSlots = 5 * 4 * (salleCount > 0 ? salleCount : 1);
        double occupationRate = (double)coursCount / totalPossibleSlots * 100;
        ViewBag.OccupationRate = Math.Min(100, Math.Round(occupationRate, 1));

        // Professeur le plus actif
        var topProf = await _context.Cours
            .GroupBy(c => c.ProfesseurId)
            .Select(g => new { ProfId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();
        
        if (topProf != null) {
            var prof = await _context.Professeurs.FindAsync(topProf.ProfId);
            ViewBag.TopProf = $"{prof?.Nom} ({topProf.Count} cours)";
        } else {
            ViewBag.TopProf = "Aucun";
        }

        // Activités récentes (5 derniers cours par ID decroissant)
        var recentActivities = await _context.Cours
            .OrderByDescending(c => c.Id)
            .Take(5)
            .ToListAsync();
        
        // On a besoin des noms des profs et salles pour l'affichage
        ViewBag.RecentActivities = recentActivities;
        ViewBag.ProfNames = await _context.Professeurs.ToDictionaryAsync(p => p.Id, p => $"{p.Nom} {p.Prenom}");
        ViewBag.SalleNames = await _context.Salles.ToDictionaryAsync(s => s.Id, s => s.Nom);

        // Assistant de planification intelligente
        var opportunities = new List<PlanningOpportunity>();
        var jours = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        var slots = new[] { 8, 10, 14, 16 };

        var allCours = await _context.Cours.ToListAsync();

        foreach (var jour in jours)
        {
            foreach (var heure in slots)
            {
                var occupiedSalleIds = allCours
                    .Where(c => c.HeureDebut.DayOfWeek == jour && c.HeureDebut.Hour == heure)
                    .Select(c => c.SalleId).ToList();
                
                var freeSalles = await _context.Salles.Where(s => !occupiedSalleIds.Contains(s.Id)).ToListAsync();

                if (freeSalles.Any())
                {
                    opportunities.Add(new PlanningOpportunity { 
                        Jour = jour, 
                        Heure = heure, 
                        SallesDisponibles = freeSalles.Select(s => s.Nom).ToList() 
                    });
                }
                if (opportunities.Count >= 4) break;
            }
            if (opportunities.Count >= 4) break;
        }

        ViewBag.Opportunities = opportunities;
        return View();
    }

    public class PlanningOpportunity {
        public DayOfWeek Jour { get; set; }
        public int Heure { get; set; }
        public List<string> SallesDisponibles { get; set; }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
