namespace GestionSalleEmploiTemps.Models
{
    public class Niveau
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty; // ex: L1, L2, L3...
        public string? Description { get; set; }
        
        // Foreign key to Filiere
        public int? FiliereId { get; set; }
        public Filiere? Filiere { get; set; }
    }
}
