using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Delete;

public class DeleteEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(string numEtud, string nom, string prenom, string email)
    {
        var etudiant = new Etudiant{NumEtud = numEtud, Nom = nom, Prenom = prenom, Email = email};
        await ExecuteAsync(etudiant);
    }
    public async Task ExecuteAsync(Etudiant etudiant)
    {
        await repositoryFactory.EtudiantRepository().DeleteAsync(etudiant);
        repositoryFactory.EtudiantRepository().SaveChangesAsync().Wait();
    }
    
    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Responsable) || role.Equals(Roles.Scolarite);
    }
}