using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    public async Task<Note> AffecterNoteAsync(long idEtudiant, long idUe, float valeurNote)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        if (e is null)
        {
            throw new KeyNotFoundException($"Etudiant avec idEtudiant {idEtudiant} n'existe pas");
        }
        
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        if (ue is null)
        {
            throw new KeyNotFoundException($"UE avec idUe {idUe} n'existe pas");
        }
        
        var noteToSave = new Note { IdEtudiant = idEtudiant, IdUe = idUe, Valeur = valeurNote };
        
        Context.Set<Note>().Add(noteToSave);
        e.NotesObtenues.Add(noteToSave);
        ue.Notes.Add(noteToSave);
        
        await Context.SaveChangesAsync();
        return noteToSave;
    }
    
    public async Task<Note> AffecterNoteAsync(Note note)
    {
        return await AffecterNoteAsync(note.IdEtudiant, note.IdUe, note.Valeur); 
    }
}