using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursDansUe;
using UniversiteDomain.Exceptions.UeDansParcoursExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.ParcoursDansUe;

public class AddsParcoursDansUeUseCase(IRepositoryFactory repositoryFactory)
{
    // Rajout d'une Ue dans un parcours
      public async Task<Ue> ExecuteAsync(Parcours parcours, Ue ue)
      {
          ArgumentNullException.ThrowIfNull(parcours);
          ArgumentNullException.ThrowIfNull(ue);
          return await ExecuteAsync(parcours.Id, ue.Id); 
      }  
      public async Task<Ue> ExecuteAsync(long idParcours, long idUe)
      {
          await CheckBusinessRules(idParcours, idUe); 
          return await repositoryFactory.UeRepository().AffecterParcoursAsync(idUe, idParcours);
      }

      // Rajout de plusieurs étudiants dans un parcours
      public async Task<Parcours> ExecuteAsync(Parcours parcours, List<Ue> ues)
      {
          ArgumentNullException.ThrowIfNull(ues);
          ArgumentNullException.ThrowIfNull(parcours);
          long[] idUes = ues.Select(x => x.Id).ToArray();
          return await ExecuteAsync(parcours.Id, idUes); 
      }  
      public async Task<Parcours> ExecuteAsync(long idParcours, long [] idUes)
      { 
        // Comme demandé par le client, on teste tous les règles avant de modifier les données
        foreach(var id in idUes) await CheckBusinessRules(idParcours, id);
        return await repositoryFactory.ParcoursRepository().AffecterUeToParcoursAsync(idUes, idParcours);
      }   

    private async Task CheckBusinessRules(long idParcours, long idUe)
    {
        // Vérification des paramètres
        ArgumentNullException.ThrowIfNull(idParcours);
        ArgumentNullException.ThrowIfNull(idUe);
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idParcours);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);
        
        // Vérifions tout d'abord que nous sommes bien connectés aux datasources
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());
        
        // On recherche l'ue
        List<Ue> ue = await repositoryFactory.UeRepository().FindByConditionAsync(e=>e.Id.Equals(idUe));;
        if (ue ==null) throw new UeNotFoundException(idUe.ToString());
        // On recherche le parcours
        List<Parcours> parcours = await repositoryFactory.ParcoursRepository().FindByConditionAsync(p=>p.Id.Equals(idParcours));;
        if (parcours ==null) throw new ParcoursNotFoundException(idParcours.ToString());
        
        // On vérifie que le Parcours n'est pas déjà dans l'UE
        if (ue[0].EnseigneeDans!=null)
        {
            // Des parcours sont déjà enregistrées dans l'UE
            // On recherche si le parcours qu'on veut ajouter n'existe pas déjà
            List<Parcours> enseigneeDansCesParcours = ue[0].EnseigneeDans;    
            var trouve=enseigneeDansCesParcours.FindAll(e=>e.Id.Equals(idParcours));
            if (trouve is { Count: > 0 }) throw new DuplicateParcoursDansUeException(idParcours+" est déjà présent dans l'UE : idUe");   
        }
    }
    
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}