using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Data;
using GestionSalleEmploiTemps.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

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
        new Niveau { Nom = "L1 Informatique", Description = "Developpement d'application internet et intranet" },
        new Niveau { Nom = "L2 Informatique", Description = "Developpement d'application internet et intranet" },
        new Niveau { Nom = "L3 Informatique", Description = "Developpement d'application internet et intranet" },
        new Niveau { Nom = "L1 Management", Description = "Administration economique et sociale" },
        new Niveau { Nom = "L2 Management", Description = "Administration economique et sociale" },
        new Niveau { Nom = "L3 Management", Description = "Administration economique et sociale" },
        new Niveau { Nom = "L1 Multimedia", Description = "Information, communication et multimedia" },
        new Niveau { Nom = "L2 Multimedia", Description = "Information, communication et multimedia" },
        new Niveau { Nom = "L3 Multimedia", Description = "Information, communication et multimedia" }
    };

    var legacyMaster1 = context.Niveaux.FirstOrDefault(n => n.Nom == "Master 1");
    if (legacyMaster1 != null && !context.Niveaux.Any(n => n.Nom == "L1 Management"))
    {
        legacyMaster1.Nom = "L1 Management";
        legacyMaster1.Description = "Administration economique et sociale";
    }

    var legacyMaster2 = context.Niveaux.FirstOrDefault(n => n.Nom == "Master 2");
    if (legacyMaster2 != null && !context.Niveaux.Any(n => n.Nom == "L2 Management"))
    {
        legacyMaster2.Nom = "L2 Management";
        legacyMaster2.Description = "Administration economique et sociale";
    }

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

    if (!context.Salles.Any())
    {
        var niveaux = context.Niveaux.ToList();
        var listSalles = new List<Salle>();

        listSalles.Add(new Salle { Nom = "Bureau Admin Main", Batiment = "A", TypeSalle = "Administration", Capacite = 10, EstDisponible = true });
        listSalles.Add(new Salle { Nom = "Bibliothèque Centrale", Batiment = "A", TypeSalle = "Bibliothèque", Capacite = 100, EstDisponible = true });
        listSalles.Add(new Salle { Nom = "Salle A01", Batiment = "A", TypeSalle = "Salle de classe", Capacite = 30, EstDisponible = true });
        listSalles.Add(new Salle { Nom = "Salle A02", Batiment = "A", TypeSalle = "Salle de classe", Capacite = 30, EstDisponible = true });

        for (int i = 1; i <= 14; i++)
        {
            var salle = new Salle { Nom = $"Salle B{i:D2}", Batiment = "B", TypeSalle = "Salle de classe", Capacite = 40, EstDisponible = true };
            if (i <= 5 && i <= niveaux.Count) salle.NiveauId = niveaux[i-1].Id;
            listSalles.Add(salle);
        }

        listSalles.Add(new Salle { Nom = "Amphi C-Grand", Batiment = "C", TypeSalle = "Amphithéâtre", Capacite = 200, EstDisponible = true });
        listSalles.Add(new Salle { Nom = "Salle C01", Batiment = "C", TypeSalle = "Salle de classe", Capacite = 35, EstDisponible = true });
        listSalles.Add(new Salle { Nom = "Salle C02", Batiment = "C", TypeSalle = "Salle de classe", Capacite = 35, EstDisponible = true });

        for (int i = 1; i <= 4; i++)
        {
            listSalles.Add(new Salle { Nom = $"Salle D{i:D2}", Batiment = "D", TypeSalle = "Salle de classe", Capacite = 30, EstDisponible = true });
        }
        listSalles.Add(new Salle { Nom = "Labo 3D Immersion", Batiment = "D", TypeSalle = "Salle 3D", Capacite = 15, EstDisponible = true });

        context.Salles.AddRange(listSalles);
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
