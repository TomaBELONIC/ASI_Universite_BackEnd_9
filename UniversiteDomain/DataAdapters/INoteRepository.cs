using UniversiteDomain.Entities;

namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task<Note> AddNoteAsync(Note note);
    
    Task<Note> AddNoteAsync(long IdEtudiant, long IdUe, float note);
}