using Microsoft.EntityFrameworkCore;
using GestionSalleEmploiTemps.Models;

namespace GestionSalleEmploiTemps.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Professeur> Professeurs { get; set; }

        public DbSet<Salle> Salles { get; set; }

        public DbSet<Cours> Cours { get; set; }

        public DbSet<Niveau> Niveaux { get; set; }

        public DbSet<Filiere> Filieres { get; set; }

        public DbSet<Examen> Examens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
   base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<Cours>()
        .HasOne(c => c.Professeur)
        .WithMany()
        .HasForeignKey(c => c.ProfesseurId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Examen>()
        .HasOne(e => e.Cours)
        .WithMany()
        .HasForeignKey(e => e.CoursId)
        .OnDelete(DeleteBehavior.Restrict);
}
    }
}