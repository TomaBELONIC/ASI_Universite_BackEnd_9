using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IParcoursRepository : IRepository<Parcours>
{
    Task<Parcours> AffecterEtudiantToParcoursAsync(Etudiant etudiant, Parcours parcours);
    Task<Parcours> AffecterEtudiantToParcoursAsync(long idEtudiant, long idParcours);
    
    Task<Parcours> AffecterEtudiantToParcoursAsync(List<Etudiant> etudiants, Parcours parcours);
    Task<Parcours> AffecterEtudiantToParcoursAsync(long[] idEtudiants, long idParcours);
    
    
    Task<Parcours> AffecterUeToParcoursAsync(Ue ue, Parcours parcours);
    Task<Parcours> AffecterUeToParcoursAsync(List<Ue> ue, Parcours parcours);
    
    Task<Parcours> AffecterUeToParcoursAsync(long idUe, long idParcours);
    Task<Parcours> AffecterUeToParcoursAsync(long[] idUes, long idParcours);
}