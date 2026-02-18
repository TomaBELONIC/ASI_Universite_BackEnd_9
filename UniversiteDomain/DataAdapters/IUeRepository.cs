using UniversiteDomain.Entities;
using Microsoft.AspNetCore.Http;
namespace UniversiteDomain.DataAdapters;

public interface IUeRepository : IRepository<Ue>
{
    Task<Ue> AffecterParcoursAsync(long idUe, long idParcours);

    Task<Ue> AffecterParcoursAsync(Ue ue, Parcours parcours);
    
    public long[] GetUeParcours(long idUe);

    public void CreerLeFichierNotesPourCetteUe(Ue ue, List<DonneesFichierCsv> infos);

    public (List<DonneesFichierCsv> Donnees, List<string> Erreurs) LireLeFichierNotesPourCetteUe(Ue ue, IFormFile fichierImporte);
}       