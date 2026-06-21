using System;
using System.ComponentModel.DataAnnotations;

namespace GestionSalleEmploiTemps.Models
{
    public class Professeur
    {
        public int Id { get; set; }

        // --- Informations personnelles ---
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

        public string? Sexe { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        public string Telephone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Adresse { get; set; }

        // --- Informations professionnelles ---
        public string? Matricule { get; set; }
        public string? Statut { get; set; } // Fonctionnaire, Vacataire, Stagiaire, Contractuel
        public string? Fonction { get; set; } // Enseignant, Chef de Mention, etc.
        public string? Departement { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateRecrutement { get; set; }

        // Ancienneté calculée
        public int? Anciennete 
        {
            get 
            {
                if (!DateRecrutement.HasValue) return null;
                var age = DateTime.Today.Year - DateRecrutement.Value.Year;
                if (DateRecrutement.Value.Date > DateTime.Today.AddYears(-age)) age--;
                return age;
            }
        }

        // --- Informations académiques ---
        public string? Diplome { get; set; }
        public string? TitreAcademique { get; set; }
        public string? DomaineSpecialisation { get; set; }

        // --- Informations du compte ---
        public string? NomUtilisateur { get; set; }
        public string MotDePasse { get; set; } = string.Empty;
        public string Role { get; set; } = "Professeur";
        public DateTime? DerniereConnexion { get; set; }
        public bool EstActif { get; set; } = true;
    }
}
