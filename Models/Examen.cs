using System;
using System.ComponentModel.DataAnnotations;

namespace GestionSalleEmploiTemps.Models
{
    public class Examen
    {
        public int Id { get; set; }

        [Required]
        public int CoursId { get; set; }

        public Cours Cours { get; set; } = null!;

        [Required]
        [Display(Name = "Date de l'examen")]
        public DateTime DateExamen { get; set; }

        [Display(Name = "Examen réussi")]
        public bool EstReussi { get; set; }

        [Display(Name = "Commentaire")]
        public string? Commentaire { get; set; }

        [Display(Name = "Date de validation")]
        public DateTime? DateValidation { get; set; }

        [Display(Name = "Validé par")]
        public int? ValidateurId { get; set; }

        
    }
}
