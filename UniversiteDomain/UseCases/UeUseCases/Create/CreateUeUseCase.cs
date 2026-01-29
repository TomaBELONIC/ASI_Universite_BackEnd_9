using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;


public class CreateUeUseCase(IUeRepository ueRepository)
{
    public async Task<Ue> ExecuteAsync(string numUe, string intitule)
    {
        var ue = new Ue{NumeroUe = numUe, Intitule = intitule,};
        return await ExecuteAsync(ue);
    }
    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        Ue et = await ueRepository.CreateAsync(ue);
        ueRepository.SaveChangesAsync().Wait();
        return et;
    }
    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
        ArgumentNullException.ThrowIfNull(ueRepository);
        
        // On recherche une Ue avec le même NumeroUe
        List<Ue> existe = await ueRepository.FindByConditionAsync(e=>e.NumeroUe.Equals(ue.NumeroUe));

        // Si une Ue avec le même numéro Ue existe déjà, on lève une exception personnalisée
        if (existe is {Count:>0}) throw new DuplicateNumUeException(ue.NumeroUe+ " - ce numéro d'UE est déjà affecté à une UE");
        
        // Le métier définit que les nom doite contenir plus de 3 lettres
        if (ue.Intitule.Length < 3) throw new InvalidIntituleUeException(ue.Intitule +" incorrect - L'intitulé d'une UE doit contenir plus de 3 caractères");
    }
}