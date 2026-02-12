using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;
using UniversiteDomain.UseCases.EtudiantUseCases.NoteAEtudiant;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

namespace UniversiteDomainUnitTests;

public class EtudiantUnitTest
{
    [SetUp]
    public void Setup()
    {
    }
    [Test]
    public async Task CreateEtudiantUseCase()
    {
        long id = 1;
        String numEtud = "et1";
        string nom = "Durant";
        string prenom = "Jean";
        string email = "jean.durant@etud.u-picardie.fr";
        
        // On crée l'étudiant qui doit être ajouté en base
        Etudiant etudiantSansId = new Etudiant{NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        //  Créons le mock du repository
        // On initialise une fausse datasource qui va simuler un EtudiantRepository
        var mock = new Mock<IRepositoryFactory>();
        // Il faut ensuite aller dans le use case pour voir quelles fonctions simuler
        // Nous devons simuler FindByCondition et Create
        
        // Simulation de la fonction FindByCondition
        // On dit à ce mock que l'étudiant n'existe pas déjà
        // La réponse à l'appel FindByCondition est donc une liste vide
        var reponseFindByCondition = new List<Etudiant>();
        // On crée un bouchon dans le mock pour la fonction FindByCondition
        // Quelque soit le paramètre de la fonction FindByCondition, on renvoie la liste vide
        mock.Setup(repo=>repo.EtudiantRepository().FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>())).ReturnsAsync(reponseFindByCondition);
        
        // Simulation de la fonction Create
        // On lui dit que l'ajout d'un étudiant renvoie un étudiant avec l'Id 1
        Etudiant etudiantCree =new Etudiant{Id=id,NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        mock.Setup(repoEtudiant=>repoEtudiant.EtudiantRepository().CreateAsync(etudiantSansId)).ReturnsAsync(etudiantCree);
        
        // On crée le bouchon (un faux etudiantRepository). Il est prêt à être utilisé
        var fauxEtudiantRepository = mock.Object;
        
        // Création du use case en injectant notre faux repository
        CreateEtudiantUseCase useCase=new CreateEtudiantUseCase(fauxEtudiantRepository);
        // Appel du use case
        var etudiantTeste=await useCase.ExecuteAsync(etudiantSansId);
        
        // Vérification du résultat
        Assert.That(etudiantTeste.Id, Is.EqualTo(etudiantCree.Id));
        Assert.That(etudiantTeste.NumEtud, Is.EqualTo(etudiantCree.NumEtud));
        Assert.That(etudiantTeste.Nom, Is.EqualTo(etudiantCree.Nom));
        Assert.That(etudiantTeste.Prenom, Is.EqualTo(etudiantCree.Prenom));
        Assert.That(etudiantTeste.Email, Is.EqualTo(etudiantCree.Email));
    }
    
    [Test]
     public async Task AddNoteAEtudiant()
    {
        // On va créer un étudiant, un parcours avec 2 UES et on va attribuer une note pour chacune
        // de ces UEs puis on va essayer de mettre une note à l'UE 1 qu'on a attribué pour l'étudiant
        //On a créer un parcours et 2 UEs, maintenant on va créer un étudiant et des notes

        long IdUe1 = 1;
        long IdEtudiant = 1;
        long ValeurNote = 18;
        
        String numEtud = "et1";
        string nom = "Durant";
        string prenom = "Jean";
        string email = "jean.durant@etud.u-picardie.fr";
        
        Ue ue1 = new Ue { Id = 1, Intitule = "ASIt", NumeroUe = "INFO_04" };
        Parcours parcoursInitial = new Parcours{Id=3, NomParcours = "MIAGE", AnneeFormation = 1};
        
        Etudiant etudiantSansId = new Etudiant{NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        
        var mockRepositoryFactory = new Mock<IRepositoryFactory>();
        var mockUeRepository = new Mock<IUeRepository>();
        var mockEtudiantRepository = new Mock<IEtudiantRepository>();
        var mockNoteRepository = new Mock<INoteRepository>();
        var reponseFindByCondition = new List<Etudiant>();
        
        mockEtudiantRepository.Setup(repo=>repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>())).ReturnsAsync(reponseFindByCondition);
        
        Etudiant etudiantCree =new Etudiant{Id=IdEtudiant,NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email, ParcoursSuivi = parcoursInitial};
        mockEtudiantRepository.Setup(repoEtudiant=>repoEtudiant.CreateAsync(etudiantSansId)).ReturnsAsync(etudiantCree);
        
        var fauxEtudiantRepository = mockRepositoryFactory.Object;
        
        CreateEtudiantUseCase useCaseEtudiant=new CreateEtudiantUseCase(fauxEtudiantRepository);
        // Appel du use case
        var etudiantTeste=await useCaseEtudiant.ExecuteAsync(etudiantSansId);

        
        Note Note1 = new Note{Etudiant = etudiantTeste, Ue = ue1, IdEtudiant = etudiantTeste.Id, IdUe = IdUe1, Valeur = ValeurNote};
        
        etudiantTeste.NotesObtenues.Add(Note1);
        
        List<Ue> ues = new List<Ue>();
        ues.Add(ue1);
        mockUeRepository.Setup(repo=>repo.FindByConditionAsync(u=>u.Id.Equals(IdUe1))).ReturnsAsync(ues);

        List<Note> lesNotesDelEtudiantInitiales = new List<Note>();
        lesNotesDelEtudiantInitiales.Add(Note1);
        
        List<Note> lesNotesDelEtudiantAModifier = new List<Note>();
        lesNotesDelEtudiantAModifier.Add(Note1);
        
        mockNoteRepository
            .Setup(repo=>repo.FindByConditionAsync(n=> (n.IdUe.Equals(IdUe1) && (n.IdEtudiant.Equals(IdEtudiant)))))
            .ReturnsAsync(lesNotesDelEtudiantInitiales);

        mockNoteRepository
            .Setup(repo => repo.AffecterNoteAsync(IdEtudiant, IdUe1, ValeurNote))
            .ReturnsAsync(Note1);
        
        // Création d'une fausse factory qui contient les faux repositories
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto=>facto.EtudiantRepository()).Returns(mockEtudiantRepository.Object);
        mockFactory.Setup(facto=>facto.NoteRepository()).Returns(mockNoteRepository.Object);
        
        // Création du use case en utilisant le mock comme datasource
        AddNoteAEtudiantUseCase useCase=new AddNoteAEtudiantUseCase(mockFactory.Object);
        
        // Appel du use case pour l'ajout de la note
        var noteTest=await useCase.ExecuteAsync(IdEtudiant, IdUe1, ValeurNote);
        // Vérification du résultat
        Assert.That(noteTest.IdEtudiant, Is.EqualTo(Note1.IdEtudiant));
        Assert.That(noteTest.IdUe, Is.EqualTo(Note1.IdUe));
        Assert.That(noteTest.Ue, Is.EqualTo(ue1));
        
        Assert.That(noteTest, Is.Not.Null);
        Assert.That(noteTest.Valeur, Is.EqualTo(ValeurNote));
        Assert.That(etudiantTeste.NotesObtenues[0], Is.EqualTo(noteTest));
    }

}