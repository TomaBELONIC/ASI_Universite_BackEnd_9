using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    public async Task<Note> AffecterNoteAsync(long idEtudiant, long idUe, decimal valeurNote)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        ArgumentNullException.ThrowIfNull(Context.Notes);
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        
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
    
    
    // public async Task<Note> ModifierNoteAsync(long idEtudiant, long idUe, decimal valeurNote)
    // {
    //     ArgumentNullException.ThrowIfNull(Context.Etudiants);
    //     ArgumentNullException.ThrowIfNull(Context.Ues);
    //     ArgumentNullException.ThrowIfNull(Context.Notes);
    //     // Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
    //     Ue ue = (await Context.Ues.FindAsync(idUe))!;
    //     
    //     var noteToSave = new Note { IdEtudiant = idEtudiant, IdUe = idUe, Valeur = valeurNote };
    //     
    //     var noteExistante = await context.Notes.FirstOrDefaultAsync(n => n.IdEtudiant == idEtudiant && n.IdUe == ue.Id);
    //     
    //     if (noteExistante != null)
    //     {
    //         if (noteExistante.Valeur.Equals(valeurNote))
    //         {
    //             return noteExistante;
    //         }
    //         else
    //         {
    //             noteExistante.Valeur = valeurNote;
    //         }
    //     }
    //     else
    //     {
    //         Context.Set<Note>().Add(noteToSave);
    //     }
    //     
    //     await Context.SaveChangesAsync();
    //     return noteToSave;
    // }
    
    public async Task<Note> ModifierNoteAsync(long idEtudiant, long idUe, decimal valeurNote)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        ArgumentNullException.ThrowIfNull(Context.Notes);
        
        var noteExistante = await Context.Notes
            .FirstOrDefaultAsync(n => n.IdEtudiant == idEtudiant && n.IdUe == idUe);
        
            noteExistante.Valeur = valeurNote;
            await Context.SaveChangesAsync();

        return noteExistante;
    }
    
    public async Task<Note> ModifierNoteAsync(Note note)
    {
        return await ModifierNoteAsync(note.IdEtudiant, note.IdUe, note.Valeur); 
    }
}