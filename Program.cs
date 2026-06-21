using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole("Professeur", "Admin", "Personnel")
        .Build();
});

var app = builder.Build();

// Créer la base de données et peupler
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = new PasswordHasher<Professeur>();

    string HashPassword(string password) => passwordHasher.HashPassword(new Professeur(), password);
    
    // S'assurer que le schéma est à jour (Note: EnsureCreated ne gère pas bien les migrations changeantes, mais ici c'est ok pour le dev)
    context.Database.EnsureCreated();

    var emitNiveaux = new List<Niveau>
    {
        new Niveau { Nom = "DA2I L1 Groupe A", Description = "Informatique" },
        new Niveau { Nom = "DA2I L1 Groupe B", Description = "Informatique" },
        new Niveau { Nom = "DA2I L2", Description = "Informatique" },
        new Niveau { Nom = "DA2I L3", Description = "Informatique" },
        new Niveau { Nom = "AES L1", Description = "Management" },
        new Niveau { Nom = "AES L2", Description = "Management" },
        new Niveau { Nom = "ICM L1", Description = "Communication" },
        new Niveau { Nom = "ICM L2", Description = "Communication" },
        new Niveau { Nom = "ICM L3", Description = "Communication" }
    };

    foreach (var emitNiveau in emitNiveaux)
    {
        var existingNiveau = context.Niveaux.FirstOrDefault(n => n.Nom == emitNiveau.Nom);
        if (existingNiveau == null)
        {
            context.Niveaux.Add(emitNiveau);
        }
        else
        {
            existingNiveau.Description = emitNiveau.Description;
        }
    }
    context.SaveChanges();

    if (!context.Professeurs.Any(p => p.Nom == "José"))
    {
        context.Professeurs.AddRange(new List<Professeur>
        {
            new Professeur { Nom = "ADMIN", Prenom = "System", Email = "admin@edu.com", Telephone = "0000000", MotDePasse = HashPassword("admin123"), Role = "Admin" },
            new Professeur { Nom = "José", Prenom = "Md", Email = "jose@edu.com", Telephone = "0340000010", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Harisetra", Prenom = "Dr", Email = "harisetra@edu.com", Telephone = "0340000011", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Valerien", Prenom = "Mr", Email = "valerien@edu.com", Telephone = "0340000012", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Jacque", Prenom = "Dr", Email = "jacque@edu.com", Telephone = "0340000013", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Brise", Prenom = "Dr", Email = "brise@edu.com", Telephone = "0340000014", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Bakari", Prenom = "Mr", Email = "bakari@edu.com", Telephone = "0340000015", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Hery", Prenom = "Dr", Email = "hery@edu.com", Telephone = "0340000016", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Fanomezana", Prenom = "Mr", Email = "fanomezana@edu.com", Telephone = "0340000017", MotDePasse = HashPassword("prof123"), Role = "Professeur" },
            new Professeur { Nom = "Raojery", Prenom = "Dr", Email = "raojery@edu.com", Telephone = "0340000018", MotDePasse = HashPassword("prof123"), Role = "Professeur" }
        });
        context.SaveChanges();
    }

    // Mise à jour des salles selon les nouvelles spécifications
    
    // Nettoyage des anciennes salles (ex: Salle B01, Salle C01, Salle D01, etc.)
    var anciennesSalles = context.Salles.Where(s => (s.Batiment == "B" || s.Batiment == "C" || s.Batiment == "D") && s.Nom.StartsWith("Salle ")).ToList();
    if (anciennesSalles.Any())
    {
        context.Salles.RemoveRange(anciennesSalles);
        context.SaveChanges();
    }

    if (!context.Salles.Any(s => s.Nom == "B001"))
    {
        // Optionnel : on pourrait vider la table Salles ici, mais attention aux clés étrangères dans Cours
        // Pour l'instant on ajoute juste les nouvelles salles si elles n'existent pas
        
        var nouvellesSalles = new List<Salle>();

        // Batiment B
        for (int etage = 0; etage <= 4; etage++)
        {
            for (int num = 1; num <= 3; num++)
            {
                string nomSalle = $"B{etage}0{num}";
                if (!context.Salles.Any(s => s.Nom == nomSalle))
                {
                    nouvellesSalles.Add(new Salle { Nom = nomSalle, Batiment = "B", TypeSalle = "Salle de classe", Capacite = 40, EstDisponible = true });
                }
            }
        }

        // Batiment A (Bureaux administratifs)
        var bureauxA = new string[] 
        {
            "Bureau directeur EMIT",
            "Bureau de chef scolarité",
            "Bureau chef de mention licence (Informatique, Management, Communication)",
            "Bureau chef de mention master (Informatique, Management, Communication)",
            "Bureau de responsable par chaque mention",
            "Bureau responsable matériel",
            "Salle matériel",
            "Bureau accueil réception",
            "Bureau de dépôt de dossier"
        };

        foreach (var bureau in bureauxA)
        {
            if (!context.Salles.Any(s => s.Nom == bureau))
            {
                nouvellesSalles.Add(new Salle { Nom = bureau, Batiment = "A", TypeSalle = "Bureau", Capacite = 5, EstDisponible = true });
            }
        }

        // Batiment C (Pas d'étage, rez-de-chaussée uniquement)
        for (int num = 1; num <= 4; num++)
        {
            string nomSalle = $"C00{num}";
            if (!context.Salles.Any(s => s.Nom == nomSalle))
            {
                nouvellesSalles.Add(new Salle { Nom = nomSalle, Batiment = "C", TypeSalle = "Salle de classe", Capacite = 40, EstDisponible = true });
            }
        }

        // Batiment D (Amphithéâtre et 1er étage)
        string amphi = "Amphithéâtre";
        if (!context.Salles.Any(s => s.Nom == amphi))
        {
            nouvellesSalles.Add(new Salle { Nom = amphi, Batiment = "D", TypeSalle = "Amphithéâtre", Capacite = 200, EstDisponible = true });
        }

        string[] sallesD = { "D101", "D102" };
        foreach (var salleD in sallesD)
        {
            if (!context.Salles.Any(s => s.Nom == salleD))
            {
                nouvellesSalles.Add(new Salle { Nom = salleD, Batiment = "D", TypeSalle = "Grande salle", Capacite = 60, EstDisponible = true });
            }
        }

        if (nouvellesSalles.Any())
        {
            context.Salles.AddRange(nouvellesSalles);
            context.SaveChanges();
        }

        // Assignation des salles spécifiques aux niveaux
        var assignations = new Dictionary<string, string>
        {
            { "B003", "DA2I L1 Groupe A" },
            { "B103", "DA2I L1 Groupe B" },
            { "B203", "DA2I L2" },
            { "B303", "DA2I L3" },
            { "Amphithéâtre", "ICM L1" },
            { "D101", "AES L1" },
            { "B101", "AES L2" },
            { "B202", "ICM L2" },
            { "B102", "ICM L3" }
        };

        foreach (var assignation in assignations)
        {
            var salle = context.Salles.FirstOrDefault(s => s.Nom == assignation.Key);
            var niveau = context.Niveaux.FirstOrDefault(n => n.Nom == assignation.Value);

            if (salle != null && niveau != null)
            {
                salle.NiveauId = niveau.Id;
            }
        }
        context.SaveChanges();
    }

    if (!context.Cours.Any(c => c.Matiere == "Anglais"))
    {
        var profs = context.Professeurs.ToDictionary(p => p.Nom, p => p.Id);
        var salles = context.Salles.Where(s => s.TypeSalle == "Salle de classe").ToList();
        var niveau = context.Niveaux.FirstOrDefault(n => n.Nom.Contains("L3")) ?? context.Niveaux.First();

        DateTime GetNextWeekday(DayOfWeek day)
        {
            DateTime start = DateTime.Today;
            while (start.DayOfWeek != day) start = start.AddDays(1);
            return start;
        }

        var schedule = new List<Cours>
        {
            // Lundi
            new Cours { Matiere = "Anglais", ProfesseurId = profs["José"], SalleId = salles[0].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Monday).AddHours(10), HeureFin = GetNextWeekday(DayOfWeek.Monday).AddHours(12) },
            new Cours { Matiere = "C++", ProfesseurId = profs["Harisetra"], SalleId = salles[1].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Monday).AddHours(14), HeureFin = GetNextWeekday(DayOfWeek.Monday).AddHours(18) },
            // Mardi
            new Cours { Matiere = "Dev mobile", ProfesseurId = profs["Valerien"], SalleId = salles[2].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Tuesday).AddHours(8), HeureFin = GetNextWeekday(DayOfWeek.Tuesday).AddHours(12) },
            new Cours { Matiere = "Javascript avancé", ProfesseurId = profs["Jacque"], SalleId = salles[3].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Tuesday).AddHours(14), HeureFin = GetNextWeekday(DayOfWeek.Tuesday).AddHours(18) },
            // Mercredi
            new Cours { Matiere = "ASP.Net", ProfesseurId = profs["Brise"], SalleId = salles[4].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Wednesday).AddHours(8), HeureFin = GetNextWeekday(DayOfWeek.Wednesday).AddHours(10) },
            new Cours { Matiere = "CPI", ProfesseurId = profs["Bakari"], SalleId = salles[5].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Wednesday).AddHours(10), HeureFin = GetNextWeekday(DayOfWeek.Wednesday).AddHours(12) },
            // Jeudi
            new Cours { Matiere = "Dev mobile", ProfesseurId = profs["Valerien"], SalleId = salles[0].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Thursday).AddHours(7), HeureFin = GetNextWeekday(DayOfWeek.Thursday).AddHours(10) },
            new Cours { Matiere = "Java web", ProfesseurId = profs["Hery"], SalleId = salles[1].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Thursday).AddHours(10), HeureFin = GetNextWeekday(DayOfWeek.Thursday).AddHours(12) },
            new Cours { Matiere = "Programmation de reseaux", ProfesseurId = profs["Harisetra"], SalleId = salles[2].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Thursday).AddHours(12), HeureFin = GetNextWeekday(DayOfWeek.Thursday).AddHours(16) },
            // Vendredi
            new Cours { Matiere = "Algebre", ProfesseurId = profs["Fanomezana"], SalleId = salles[3].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Friday).AddHours(7), HeureFin = GetNextWeekday(DayOfWeek.Friday).AddHours(12) },
            new Cours { Matiere = "COE", ProfesseurId = profs["Raojery"], SalleId = salles[4].Id, NiveauId = niveau.Id, HeureDebut = GetNextWeekday(DayOfWeek.Friday).AddHours(14), HeureFin = GetNextWeekday(DayOfWeek.Friday).AddHours(17) }
        };

        context.Cours.AddRange(schedule);
        context.SaveChanges();
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
