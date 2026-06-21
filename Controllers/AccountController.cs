using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;

using Microsoft.AspNetCore.Authorization;

namespace GestionSalleEmploiTemps.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Professeur> _passwordHasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var prof = await _context.Professeurs
                .FirstOrDefaultAsync(p => p.Email == email || p.Matricule == email);

            if (prof != null && IsValidPassword(prof, password))
            {
                if (!prof.EstActif)
                {
                    ViewBag.Error = "Votre compte est inactif. Veuillez contacter l'administrateur.";
                    return View();
                }

                prof.DerniereConnexion = DateTime.Now;
                _context.Update(prof);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, prof.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{prof.Nom} {prof.Prenom}"),
                    new Claim(ClaimTypes.Email, prof.Email),
                    new Claim(ClaimTypes.Role, prof.Role ?? "Professeur")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email ou mot de passe incorrect.";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Nom,Prenom,Email,Telephone,MotDePasse,Sexe,DateRecrutement,Statut,Fonction,Departement,Diplome,NomUtilisateur")] Professeur professeur)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Professeurs.AnyAsync(p => p.Email == professeur.Email);
                if (exists)
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                    return View(professeur);
                }

                // Génération du matricule
                int ordre = await _context.Professeurs.CountAsync() + 1;
                string sexe = professeur.Sexe == "H" ? "H" : (professeur.Sexe == "F" ? "F" : "X");
                string annee = professeur.DateRecrutement.HasValue ? professeur.DateRecrutement.Value.ToString("yy") : DateTime.Now.ToString("yy");
                professeur.Matricule = $"{ordre:D3}{sexe}EMIT{annee}";
                professeur.EstActif = true;

                professeur.MotDePasse = _passwordHasher.HashPassword(professeur, professeur.MotDePasse);
                _context.Add(professeur);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Inscription réussie ! Votre matricule est : {professeur.Matricule}. Veuillez l'utiliser pour vous connecter.";
                
                return RedirectToAction(nameof(Login));
            }
            return View(professeur);
        }

        // POST: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private bool IsValidPassword(Professeur professeur, string password)
        {
            var verificationResult = _passwordHasher.VerifyHashedPassword(professeur, professeur.MotDePasse, password);
            if (verificationResult != PasswordVerificationResult.Failed)
            {
                return true;
            }

            // Compatibilite avec les comptes de demonstration deja crees en clair.
            if (professeur.MotDePasse == password)
            {
                professeur.MotDePasse = _passwordHasher.HashPassword(professeur, password);
                _context.SaveChanges();
                return true;
            }

            return false;
        }
    }
}
