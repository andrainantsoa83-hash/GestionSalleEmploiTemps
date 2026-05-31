namespace GestionSalleEmploiTemps.Models
{
    public class Professeur
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;

        public string Prenom { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telephone { get; set; } = string.Empty;
        
        public string MotDePasse { get; set; } = string.Empty;
        
        public string Role { get; set; } = "Professeur";
    }
}
