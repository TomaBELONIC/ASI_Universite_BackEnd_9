using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.NoteAEtudiantExceptions;
using UniversiteDomain.Exceptions.UeDansParcoursExceptions;

namespace UniversiteDomain.UseCases.NotesUseCases.Update;

public class UpdateNoteAEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
      public async Task<Note> ExecuteAsync(long IdEtudiant, long IdUe, decimal note)
      {
          ArgumentNullException.ThrowIfNull(IdEtudiant);
          ArgumentNullException.ThrowIfNull(IdUe);
          ArgumentNullException.ThrowIfNull(note);
          
          return await repositoryFactory.NoteRepository().ModifierNoteAsync(IdEtudiant, IdUe, note);
      }  
      public async Task<Note> ExecuteAsync(Note note)
      {
          await CheckDataSources();
          await CheckBusinessRules(note); 
          return await repositoryFactory.NoteRepository().ModifierNoteAsync(note);
      }
      
      public async Task<IReadOnlyList<Note>> ExecuteAsync(IEnumerable<Note> notes)
      {
          if (notes == null) throw new ArgumentNullException(nameof(notes));

          var lesNotes = new List<Note>();

          await CheckDataSources();

          foreach (var note in notes)
          {
              await CheckBusinessRules(note);
              var updated = await repositoryFactory.NoteRepository().ModifierNoteAsync(note);
              lesNotes.Add(updated);
          }

          return lesNotes;
      }
      
    private async Task CheckBusinessRules(Note note)
    {
        // Vérification des paramètres
        ArgumentNullException.ThrowIfNull(note.IdEtudiant);
        ArgumentNullException.ThrowIfNull(note.IdUe);
        ArgumentNullException.ThrowIfNull(note);
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(note.IdEtudiant);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(note.IdUe);
        
        // On recherche l'étudiant
        List<Etudiant> etudiant = await repositoryFactory.EtudiantRepository().FindByConditionAsync(e=>e.Id.Equals(note.IdEtudiant));;
        if (etudiant is { Count: 0 }) throw new EtudiantNotFoundException(note.IdEtudiant.ToString());
        
        // On recherche l'UE
        List<Ue> ue = await repositoryFactory.UeRepository().FindByConditionAsync(e=>e.Id.Equals(note.IdUe));;
        if (ue is { Count: 0 }) throw new UeNotFoundException(note.IdUe.ToString());
    }
    
    private async Task CheckDataSources()
    {
        // Vérifions tout d'abord que nous sommes bien connectés aux datasources
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.EtudiantRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.NoteRepository());
    }
    
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}

