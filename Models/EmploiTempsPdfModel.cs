using System;
using System.Collections.Generic;

namespace GestionSalleEmploiTemps.Models
{
    public class EmploiTempsPdfModel
    {
        public string Etablissement { get; set; } = "UNIVERSITÉ EMIT";
        public string SousTitre { get; set; } = "Gestion des emplois du temps";
        public string NiveauNom { get; set; }
        public string SemaineText { get; set; }
        public DateTime DateGeneration { get; set; } = DateTime.Now;
        public string LogoPath { get; set; }
        
        public List<Cours> CoursList { get; set; } = new List<Cours>();
    }
}