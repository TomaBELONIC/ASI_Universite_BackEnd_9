using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class UeRepository(UniversiteDbContext context) : Repository<Ue>(context), IUeRepository
{
    public async Task AffecterParcoursAsync(long idUe, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        
        ue.EnseigneeDans!.Add(p);
        await Context.SaveChangesAsync();
    }
    
    public async Task AffecterParcoursAsync(Ue ue, Parcours parcours)
    {
        await AffecterParcoursAsync(ue.Id, parcours.Id); 
    }
}