namespace GestionSalleEmploiTemps.Models
{
    public enum StatutCours
    {
        Planifie = 0,      // Cours prévu
        EnProgress = 1,    // Cours en cours
        Termine = 2,       // Cours terminé (programme fini)
        Examen = 3,        // Examen en cours
        Complete = 4       // Cours complètement terminé (examen réussi)
    }
}