namespace UniversiteDomain.Entities;

public class Etudiant
{
    public long Id { get; set; }
    public string NumEtud { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Ajout de cet attribut pour faire le lien avec l'entité Parcours
    // ManyToOne : l'étudiant est inscrit dans un parcours
    public Parcours? ParcoursSuivi { get; set; } = null;
    
    // Liste des notes d'un étudiant
    // La liste est vide aussi comme pour les autres cas, une liste null donc non allouée en mémoire ne représente rien
    public List<Note> Notes { get; set; } = new();
    
    public override string ToString()
    {
        return $"ID {Id} : {NumEtud} - {Nom} {Prenom} inscrit en " + ParcoursSuivi;
    }
}