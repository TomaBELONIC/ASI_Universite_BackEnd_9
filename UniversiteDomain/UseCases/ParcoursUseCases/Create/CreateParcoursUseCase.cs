using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Parcours> ExecuteAsync(long id, string nomParcours, int anneeFormation)
    {
        var parcours = new Parcours{Id = id, NomParcours = nomParcours, AnneeFormation = anneeFormation};
        return await ExecuteAsync(parcours);
    }
    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);
        Parcours parc = await repositoryFactory.ParcoursRepository().CreateAsync(parcours);
        repositoryFactory.ParcoursRepository().SaveChangesAsync().Wait();
        return parc;
    }
    private async Task CheckBusinessRules(Parcours parcours)
    {
        int[] lesDeuxAnneesDeMaster = new[] { 1, 2 };
        
        ArgumentNullException.ThrowIfNull(parcours);
        // ArgumentNullException.ThrowIfNull(parcours.Id);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);
        ArgumentNullException.ThrowIfNull(repositoryFactory.ParcoursRepository());
        
        // On recherche un Parcours avec le même nom et la même année de formation
        List<Parcours> existe = await repositoryFactory.ParcoursRepository().FindByConditionAsync(p=> (p.NomParcours.Equals(parcours.NomParcours)) && (p.AnneeFormation.Equals(parcours.AnneeFormation)));

        // Si un parcours avec le même nom de parcours et la même année de formation existe déjà, on lève une exception personnalisée
        if (existe is {Count:>0}) throw new DuplicateParcoursException(parcours.NomParcours+ " " + parcours.AnneeFormation + " - ce parcours existe déjà !");
        
        // Le métier définit que les nom doit contenir plus de 3 lettres
        if (parcours.NomParcours.Length < 3) throw new InvalidNomParcoursException(parcours.NomParcours +" incorrect - Le nom d'un parcours doit contenir plus de 3 caractères");
        
        // Pour l'année de formation, voir si c'est 1 ou 2
        if (!(lesDeuxAnneesDeMaster.Contains(parcours.AnneeFormation))) throw new InvalidAnneeFormationException(parcours.AnneeFormation +" incorrect - L'année de formation est soit 1 soit 2");
    }
    
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Responsable) || role.Equals(Roles.Scolarite);
    }
}