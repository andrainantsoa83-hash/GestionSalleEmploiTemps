namespace GestionSalleEmploiTemps.Models
{
    public class Niveau
    {
        public int Id { get; set; }
        
        public string Nom { get; set; } = string.Empty; // ex: L1, L2, M1, M2...
        
        public string Parcours { get; set; } = string.Empty; // ex: DA2I
        
        public string Mention { get; set; } = string.Empty; // ex: Informatique, Management, Communication
        
        public string? Description { get; set; } // Signification du parcours
        
        // Foreign key to Filiere
        public int? FiliereId { get; set; }
        public Filiere? Filiere { get; set; }
    }
}
