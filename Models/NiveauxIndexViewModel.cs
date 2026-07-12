using System.Collections.Generic;

namespace GestionSalleEmploiTemps.Models
{
    public class NiveauxIndexViewModel
    {
        public List<Niveau> Niveaux { get; set; } = new List<Niveau>();
        
        // Propriétés pour la pagination
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        
        // Propriétés pour les filtres
        public string SearchString { get; set; }
    }
}
