namespace GestionSalleEmploiTemps.Models
{
    public class Salle
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;

        public int Capacite { get; set; }

        public bool EstDisponible { get; set; }

        public string Batiment { get; set; } = string.Empty; // A, B, C, D

        public string TypeSalle { get; set; } = string.Empty; // Salle de classe, Amphi, Salle 3D, etc.

        public int? NiveauId { get; set; }
    }
}
