using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.UeUseCases.Create;

namespace UniversiteDomainUnitTests;

public class UeUnitTest
{
    [SetUp]
    public void Setup()
    {
    }
    [Test]
    public async Task CreateUeUseCase()
    {
        long id = 1;
        String numUe = "ue1";
        string intitule = "ASI";
        
        // On crée l'ue qui doit être ajouté en base
        Ue ueSansId = new Ue{NumeroUe = numUe, Intitule = intitule};
        
        //  Créons le mock du repository
        // On initialise une fausse datasource qui va simuler un UeRepository
        var mock = new Mock<IRepositoryFactory>();
        
        // Il faut ensuite aller dans le use case pour voir quelles fonctions simuler
        // Nous devons simuler FindByCondition et Create
        
        // Simulation de la fonction FindByCondition
        // On dit à ce mock que l'ue n'existe pas déjà
        
        // La réponse à l'appel FindByCondition est donc une liste vide
        var reponseFindByCondition = new List<Ue>();
        // On crée un bouchon dans le mock pour la fonction FindByCondition
        // Quelque soit le paramètre de la fonction FindByCondition, on renvoie la liste vide
        mock.Setup(repo=>repo.UeRepository().FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>())).ReturnsAsync(reponseFindByCondition);
        
        // Simulation de la fonction Create
        // On lui dit que l'ajout d'une Ue renvoie une Ue avec l'Id 1
        Ue ueCree =new Ue{Id=id, NumeroUe = numUe, Intitule = intitule};
        mock.Setup(repoUe=>repoUe.UeRepository().CreateAsync(ueSansId)).ReturnsAsync(ueCree);
        
        // On crée le bouchon (un faux ueRepository). Il est prêt à être utilisé
        var fauxUeRepository = mock.Object;
        
        // Création du use case en injectant notre faux repository
        CreateUeUseCase useCase=new CreateUeUseCase(fauxUeRepository);
        // Appel du use case
        var ueTeste=await useCase.ExecuteAsync(ueSansId);
        
        // Vérification du résultat
        Assert.That(ueTeste.Id, Is.EqualTo(ueCree.Id));
        Assert.That(ueTeste.NumeroUe, Is.EqualTo(ueCree.NumeroUe));
        Assert.That(ueTeste.Intitule, Is.EqualTo(ueCree.Intitule));
    }

}