using System;

namespace GestionSalleEmploiTemps.Models
{
    public class Cours
    {
        public int Id { get; set; }

        public string Matiere { get; set; } = string.Empty;

        public DateTime HeureDebut { get; set; }

        public DateTime HeureFin { get; set; }

        public int ProfesseurId { get; set; }
        public Professeur Professeur { get; set; } = null!;

        public int SalleId { get; set; }
        public Salle Salle { get; set; } = null!;

        public int NiveauId { get; set; }
        public Niveau Niveau { get; set; } = null!;

        public StatutCours Statut { get; set; } = StatutCours.Planifie;

        public DateTime? DateCompletion { get; set; }
    }
}
