using System;
using System.ComponentModel.DataAnnotations;

namespace GestionSalleEmploiTemps.Models
{
    public class Utilisateur
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        public string MotDePasse { get; set; }

        [Required(ErrorMessage = "Le rôle est requis.")]
        public string Role { get; set; } // "Etudiant" ou "Enseignant"
    }
}