using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NotesUseCases.Get;

public class GetNotesUeUseCase(IRepositoryFactory factory)
{
    public async Task<Ue?> ExecuteAsync(long idUe)
    {
        await CheckBusinessRules(idUe);
        Ue? ue = await factory.UeRepository().FindAsync(idUe);
        
        List<Note> notes = await factory.NoteRepository().FindByConditionAsync(n => n.IdUe == idUe);

        var parcoursIds = factory.UeRepository().GetUeParcours(idUe);
        
        List<Etudiant> etudiantsPourCetteUe = await factory.EtudiantRepository()
            .FindByConditionAsync(e => e.ParcoursSuivi != null && parcoursIds.Contains(e.ParcoursSuivi.Id));
        
        List<DonneesFichierCsv> donneesFichierCsv = new List<DonneesFichierCsv>();
            
        foreach (var etudiant in etudiantsPourCetteUe)
        {
            decimal? valeurNote = notes.FirstOrDefault(n => n.IdEtudiant == etudiant.Id)?.Valeur;
            var newTest = new DonneesFichierCsv
            {
                NumEtud = etudiant.NumEtud,
                Nom = etudiant.Nom,
                Prenom = etudiant.Prenom,
                Note = valeurNote
            };
            donneesFichierCsv.Add(newTest);
        }
        
        factory.UeRepository().CreerLeFichierNotesPourCetteUe(ue, donneesFichierCsv);
        
        return ue;
    }
    
    private async Task CheckBusinessRules(long idUe)
    {
        ArgumentNullException.ThrowIfNull(factory);
        
        IUeRepository ueRepository = factory.UeRepository();
        ArgumentNullException.ThrowIfNull(ueRepository);
        
        ArgumentNullException.ThrowIfNull(idUe);
        Ue ue = await factory.UeRepository().FindAsync(idUe);
        ArgumentNullException.ThrowIfNull(ue);
        
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        ArgumentNullException.ThrowIfNull(etudiantRepository);
        
        INoteRepository noteRepository = factory.NoteRepository();
        ArgumentNullException.ThrowIfNull(noteRepository);
    }
    
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}