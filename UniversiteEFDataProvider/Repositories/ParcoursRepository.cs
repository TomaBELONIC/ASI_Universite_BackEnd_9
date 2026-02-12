using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository(UniversiteDbContext context) : Repository<Parcours>(context), IParcoursRepository
{
    // Etudiant à Parcours
    public async Task<Parcours> AffecterEtudiantToParcoursAsync(long idEtudiant, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        
        p.Inscrits!.Add(e);
        await Context.SaveChangesAsync();
        return p;
    }
    public async Task<Parcours> AffecterEtudiantToParcoursAsync(Etudiant etudiant, Parcours parcours)
    {
        return await AffecterEtudiantToParcoursAsync(etudiant.Id, parcours.Id); 
    }
    
    // avec une liste d'étudiants
    public async Task<Parcours> AffecterEtudiantToParcoursAsync(long[] idEtudiants, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        
        Etudiant e;
        
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;

        foreach (var idEtudiant in idEtudiants)
        {
            e = (await Context.Etudiants.FindAsync(idEtudiant))!;
            p.Inscrits!.Add(e);
        }
        
        await Context.SaveChangesAsync();
        return p;
    }
    public async Task<Parcours> AffecterEtudiantToParcoursAsync(List<Etudiant> etudiants, Parcours parcours)
    {
        var etudiantIds = etudiants.Select(e => e.Id).ToArray();
        return await AffecterEtudiantToParcoursAsync(etudiantIds, parcours.Id); 
    }
    
    
    // Ue à Parcours
    public async Task<Parcours> AffecterUeToParcoursAsync(long idUe, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Ues);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        
        p.UesEnseignees!.Add(ue);
        await Context.SaveChangesAsync();
        return p;
    }
    public async Task<Parcours> AffecterUeToParcoursAsync(Ue ue, Parcours parcours)
    {
        return await AffecterUeToParcoursAsync(ue.Id, parcours.Id); 
    }
    
    // avec une liste d'UES
    public async Task<Parcours> AffecterUeToParcoursAsync(long[] idUes, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Ues);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        
        Ue ue;
        
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;

        foreach (var idUe in idUes)
        {
            ue = (await Context.Ues.FindAsync(idUe))!;
            p.UesEnseignees!.Add(ue);
        }
        
        await Context.SaveChangesAsync();
        return p;
    }
    public async Task<Parcours> AffecterUeToParcoursAsync(List<Ue> ues, Parcours parcours)
    {
        var idUes = ues.Select(e => e.Id).ToArray();
        return await AffecterUeToParcoursAsync(idUes, parcours.Id); 
    }
}