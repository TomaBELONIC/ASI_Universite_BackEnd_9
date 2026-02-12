using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteAEtudiantExceptions;
using UniversiteDomain.Exceptions.UeDansParcoursExceptions;

namespace UniversiteDomain.UseCases.EtudiantUseCases.NoteAEtudiant;

public class AddNoteAEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
    // Rajout d'une Note à un Etudiant
      public async Task<Note> ExecuteAsync(long IdEtudiant, long IdUe, float note)
      {
          ArgumentNullException.ThrowIfNull(IdEtudiant);
          ArgumentNullException.ThrowIfNull(IdUe);
          ArgumentNullException.ThrowIfNull(note);
          
          return await repositoryFactory.NoteRepository().AffecterNoteAsync(IdEtudiant, IdUe, note);
      }  
      public async Task<Note> ExecuteAsync(Note note)
      {
          await CheckBusinessRules(note); 
          return await repositoryFactory.NoteRepository().AffecterNoteAsync(note);
      }
      
    private async Task CheckBusinessRules(Note note)
    {
        // Vérification des paramètres
        ArgumentNullException.ThrowIfNull(note.IdEtudiant);
        ArgumentNullException.ThrowIfNull(note.IdUe);
        ArgumentNullException.ThrowIfNull(note);
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(note.IdEtudiant);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(note.IdUe);
        
        // Vérifions tout d'abord que nous sommes bien connectés aux datasources
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.EtudiantRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.NoteRepository());
        
        // On recherche l'étudiant
        List<Etudiant> etudiant = await repositoryFactory.EtudiantRepository().FindByConditionAsync(e=>e.Id.Equals(note.IdEtudiant));;
        if (etudiant is { Count: 0 }) throw new EtudiantNotFoundException(note.IdEtudiant.ToString());
        
        // On recherche l'UE
        List<Ue> ue = await repositoryFactory.UeRepository().FindByConditionAsync(e=>e.Id.Equals(note.IdUe));;
        if (ue is { Count: 0 }) throw new UeNotFoundException(note.IdUe.ToString());
        
        // On regarde si l'étudiant n'a pas déjà la note
        foreach (Note n in etudiant[0].NotesObtenues)
        {
            if ((n.IdUe.Equals(note.IdUe) && n.IdEtudiant.Equals(note.IdEtudiant)))
            {
                throw new DuplicateNotePourUePourEtudiantException(note.IdEtudiant + " a déjà cette note pour cette UE : " + n.IdUe);
            }
        }
    }
}

