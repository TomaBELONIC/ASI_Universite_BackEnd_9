using Microsoft.AspNetCore.Http;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.NotesUseCases.Add;
using UniversiteDomain.UseCases.NotesUseCases.Update;

namespace UniversiteDomain.UseCases.NotesUseCases.Import;

public class ImportNotesUeUseCase(IRepositoryFactory factory)
{
    public async Task<Ue?> ExecuteAsync(long idUe, (List<DonneesFichierCsv> Donnees, List<string> Erreurs) donneesAMettreEnBase)
    {
        await CheckBusinessRules(idUe);
        Ue? ue = await factory.UeRepository().FindAsync(idUe);
        
        AddNoteAEtudiantUseCase addNoteAEtudiantUseCase = new AddNoteAEtudiantUseCase(factory);
        UpdateNoteAEtudiantUseCase updateNoteAEtudiantUseCase = new UpdateNoteAEtudiantUseCase(factory);
        
        if (donneesAMettreEnBase.Erreurs.Count == 0)
        {
            foreach (var donnee in donneesAMettreEnBase.Donnees)
            {
                if (donnee.Note != null)
                {
                    var idEtudiant = (await factory.EtudiantRepository()
                            .FindByConditionAsync(et => et.NumEtud == donnee.NumEtud))
                        .SingleOrDefault().Id;
                    
                    var noteExistantes = await factory.NoteRepository()
                        .FindByConditionAsync(n => n.IdEtudiant == idEtudiant && n.IdUe == ue.Id);

                    var noteExistante = noteExistantes.FirstOrDefault();
                    
                    if (noteExistantes is {Count:>0})
                    {
                        if (noteExistante.Valeur != donnee.Note)
                        {
                            noteExistante.Valeur = (decimal)donnee.Note;
                            await updateNoteAEtudiantUseCase.ExecuteAsync(noteExistante);
                        }
                    }
                    else
                    {
                        var nouvelleNote = new Note
                        {
                            IdEtudiant = idEtudiant,
                            IdUe = idUe,
                            Valeur = (decimal)donnee.Note
                        };
                        
                        await addNoteAEtudiantUseCase.ExecuteAsync(nouvelleNote);
                    }
                }
            }
        }
        
        return ue;
    }
    
    public async Task<Ue?> ExecuteAsync2(long idUe, (List<DonneesFichierCsv> Donnees, List<string> Erreurs) donneesAMettreEnBase)
    {
        await CheckBusinessRules(idUe);
        Ue? ue = await factory.UeRepository().FindAsync(idUe);
        
        AddNoteAEtudiantUseCase addNoteAEtudiantUseCase = new AddNoteAEtudiantUseCase(factory);
        UpdateNoteAEtudiantUseCase updateNoteAEtudiantUseCase = new UpdateNoteAEtudiantUseCase(factory);
        
        // listes à remplir
        var notesToAdd = new List<Note>();
        var notesToUpdate = new List<Note>();

        if (donneesAMettreEnBase.Erreurs.Count == 0)
        {
            var numEtuds = donneesAMettreEnBase.Donnees
                .Where(d => d.Note != null)
                .Select(d => d.NumEtud)
                .Distinct()
                .ToList();

            if (numEtuds.Count == 0)
                return ue;

            var etudiants = await factory.EtudiantRepository()
                .FindByConditionAsync(e => numEtuds.Contains(e.NumEtud));
            var mapNumEtudToId = etudiants.ToDictionary(e => e.NumEtud, e => e.Id);

            var etudiantIds = mapNumEtudToId.Values.ToList();
            var existingNotes = await factory.NoteRepository()
                .FindByConditionAsync(n => n.IdUe == idUe && etudiantIds.Contains(n.IdEtudiant));
            var existingByEtudiant = existingNotes.ToDictionary(n => n.IdEtudiant);

            foreach (var donnee in donneesAMettreEnBase.Donnees)
            {
                if (donnee.Note == null) continue;

                mapNumEtudToId.TryGetValue(donnee.NumEtud, out var idEtudiant);
                
                var valeurDecimal = (decimal)donnee.Note;

                if (existingByEtudiant.TryGetValue(idEtudiant, out var existingNote))
                {
                    if (existingNote.Valeur != valeurDecimal)
                    {
                        existingNote.Valeur = valeurDecimal;
                        notesToUpdate.Add(existingNote);
                    }
                }
                else
                {
                    var nouvelleNote = new Note
                    {
                        IdEtudiant = idEtudiant,
                        IdUe = idUe,
                        Valeur = valeurDecimal
                    };
                    notesToAdd.Add(nouvelleNote);
                }
            }
            
            if (notesToUpdate.Count > 0)
            {
                await updateNoteAEtudiantUseCase.ExecuteAsync(notesToUpdate);
            }
            if (notesToAdd.Count > 0)
            {
                await addNoteAEtudiantUseCase.ExecuteAsync(notesToAdd);
            }
        }
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

