using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task<Note> AffecterNoteAsync(Note note);
    
    Task<Note> AffecterNoteAsync(long idEtudiant, long idUe, decimal note);
    
    Task<Note> ModifierNoteAsync(Note note);
    
    Task<Note> ModifierNoteAsync(long idEtudiant, long idUe, decimal note);
}