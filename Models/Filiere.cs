namespace GestionSalleEmploiTemps.Models
{
    public class Filiere
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty; // ex: Informatique, Management, Multimédia
        public string? Description { get; set; } // ex: Développement d'application internet intranet
        public string? Code { get; set; } // ex: INFO, MGMT, MULTI
    }
}