using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task<Note> AffecterNoteAsync(Note note);
    
    Task<Note> AffecterNoteAsync(long idEtudiant, long idUe, float note);
}