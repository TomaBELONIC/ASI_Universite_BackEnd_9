using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;
using UniversiteDomain.UseCases.EtudiantUseCases.Delete;
using UniversiteDomain.UseCases.EtudiantUseCases.Get;
using UniversiteDomain.UseCases.SecurityUseCases.Create;
using UniversiteDomain.UseCases.SecurityUseCases.Get;
using UniversiteEFDataProvider.Entities;

namespace UniversiteRestApi.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class EtudiantController(IRepositoryFactory repositoryFactory) : ControllerBase
    {
        // GET: api/<EtudiantController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<EtudiantController>/5
        // [HttpGet("{id}")]
        // public string Get(int id)
        // {
        //     return "value";
        // }
        
        // GET api/<EtudiantController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EtudiantDto>>  GetUnEtudiant(long id)
        {
            string role="";
            string email="";
            IUniversiteUser user = null;
            try
            {
                CheckSecu(out role, out email, out user);
            }
            catch (Exception e)
            {
                return Unauthorized();
            }
            
            GetEtudiantByIdUseCase uc = new GetEtudiantByIdUseCase(repositoryFactory);
            // On vérifie si l'utilisateur connecté a le droit d'accéder à la ressource
            if (!uc.IsAuthorized(role, user, id)) return Unauthorized();
            Etudiant? etud;
            try
            {
                etud = await uc.ExecuteAsync(id);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }
           
            if (etud == null) return NotFound();
            
            return new EtudiantDto().ToDto(etud);
        }
        
        // Définissons une nouvelle route pour ne pas créer de conflit avec le get de base
        // GET api/<EtudiantController>/complet/5
        [HttpGet("complet/{id}")]
        public async Task<ActionResult<EtudiantCompletDto>> GetUnEtudiantCompletAsync(long id)
        {
            // Identification et authentification
            string role="";
            string email="";
            IUniversiteUser user = null;
            try
            {
                CheckSecu(out role, out email, out user);
            }
            catch (Exception e)
            {
                return Unauthorized();
            }
    
            GetEtudiantCompletUseCase uc = new GetEtudiantCompletUseCase(repositoryFactory);
            // Autorisation
            // On vérifie si l'utilisateur connecté a le droit d'accéder à la ressource
            if (!uc.IsAuthorized(role, user, id)) return Unauthorized();
            Etudiant? etud;
            try
            {
                etud = await uc.ExecuteAsync(id);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }
            if (etud == null) return NotFound();
            return new EtudiantCompletDto().ToDto(etud);
        }
        
        
        

        // // POST api/<EtudiantController>
        // [HttpPost]
        // public void Post([FromBody] string value)
        // {
        // }
        
        // Crée un nouvel étudiant sans parcours
        // POST api/<EtudiantApi>
        // [HttpPost]
        // public async Task PostAsync([FromBody] Etudiant etudiant)
        // {
        //     CreateEtudiantUseCase uc=new CreateEtudiantUseCase(repositoryFactory);
        //     await uc.ExecuteAsync(etudiant);
        // }
        
        // Sans DTO
        // Crée un nouvel étudiant sans parcours
        // POST api/<EtudiantApi>
        // [HttpPost]
        // public async Task<Etudiant> PostAsync([FromBody] Etudiant etudiant)
        // {
        //     CreateEtudiantUseCase uc=new CreateEtudiantUseCase(repositoryFactory);
        //     return await uc.ExecuteAsync(etudiant);
        // }

        // Maintenant en utilisant EtudiantDTO
        // POST api/<EtudiantController>
        // [HttpPost]
        // public async Task<EtudiantDto> PostAsync([FromBody] EtudiantDto etudiantDto)
        // {
        //     // Converstion d'un EtudiantDto sans Id en Etudiant
        //     Etudiant etudiant = etudiantDto.ToEntity();
        //     // Création d'un étudiant
        //     CreateEtudiantUseCase uc=new CreateEtudiantUseCase(repositoryFactory);
        //     etudiant = await uc.ExecuteAsync(etudiant);
        //     // Conversion de l'étudiant avec son nouvel Id en EtudiantDto
        //     return new EtudiantDto().ToDto(etudiant);
        // }
        
        // Avec EtudiantDTO et en personnalisant le retour des services
        // POST api/<EtudiantController>
        // [HttpPost]
        // public async Task<ActionResult<EtudiantDto>> PostAsync([FromBody] EtudiantDto etudiantDto)
        // {
        //     CreateEtudiantUseCase createEtudiantUc = new CreateEtudiantUseCase(repositoryFactory);           
        //     Etudiant etud = etudiantDto.ToEntity();
        //     try
        //     {
        //         etud = await createEtudiantUc.ExecuteAsync(etud);
        //     }
        //     catch (Exception e)
        //     {
        //         // On récupère ici les exceptions personnalisées définies dans la couche domain
        //         // Et on les envoie avec le code d'erreur 400 et l'intitulé "erreurs de validation"
        //         ModelState.AddModelError(nameof(e), e.Message);
        //         return ValidationProblem();
        //     }
        //     EtudiantDto dto = new EtudiantDto().ToDto(etud);
        //     // On revoie la route vers le get qu'on n'a pas encore écrit!
        //     return CreatedAtAction(nameof(GetUnEtudiant), new { id = dto.Id }, dto);
        // }
        
        // POST api/<EtudiantController> Chapitre 7 | 11.1 Identification et autorisation
        [HttpPost]
        public async Task<ActionResult<EtudiantDto>> PostAsync([FromBody] EtudiantDto etudiantDto)
        {
            CreateEtudiantUseCase createEtudiantUc = new CreateEtudiantUseCase(repositoryFactory);
            CreateUniversiteUserUseCase createUserUc = new CreateUniversiteUserUseCase(repositoryFactory);

            string role="";
            string email="";
            IUniversiteUser user = null;
            CheckSecu(out role, out email, out user);
            if (!createEtudiantUc.IsAuthorized(role) || !createUserUc.IsAuthorized(role)) return Unauthorized();
    
            Etudiant etud = etudiantDto.ToEntity();
    
            try
            {
                etud = await createEtudiantUc.ExecuteAsync(etud);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }

            try
            {
                // Création du user associé
                user = new UniversiteUser { UserName = etudiantDto.Email, Email = etudiantDto.Email, Etudiant = etud };
                // Un créé l'utilisateur avec un mot de passe par défaut et un rôle étudiant
                await createUserUc.ExecuteAsync(etud.Email, etud.Email, "Miage2025#", Roles.Etudiant, etud); 
            }
            catch (Exception e)
            {
                // On supprime l'étudiant que l'on vient de créer. Sinon on a un étudiant mais pas de user associé
                await new DeleteEtudiantUseCase(repositoryFactory).ExecuteAsync(etud);
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }

            EtudiantDto dto = new EtudiantDto().ToDto(etud);
            return CreatedAtAction(nameof(GetUnEtudiant), new { id = dto.Id }, dto);
        }
        
        
        
        
        
        // PUT api/<EtudiantController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        
        
        
        // DELETE api/<EtudiantController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        
        
        
        
        
        private void CheckSecu(out string role, out string email, out IUniversiteUser user)
        {
            role = "";
            // Récupération des informations de connexion dans la requête http entrante
            ClaimsPrincipal claims = HttpContext.User;
            // Faisons nos tests pour savoir si la personne est bien connectée
            if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
            // Récupérons le email de la personne connectée
            if (claims.FindFirst(ClaimTypes.Email)==null) throw new UnauthorizedAccessException();
            email = claims.FindFirst(ClaimTypes.Email).Value;
            if (email==null) throw new UnauthorizedAccessException();
            // Vérifions qu'il est bien associé à un utilisateur référencé
            user = new FindUniversiteUserByEmailUseCase(repositoryFactory).ExecuteAsync(email).Result;
            if (user==null) throw new UnauthorizedAccessException();
            // Vérifions qu'un rôle a bien été défini
            if (claims.FindFirst(ClaimTypes.Role)==null) throw new UnauthorizedAccessException();
            // Récupérons le rôle de l'utilisateur
            var ident = claims.Identities.FirstOrDefault();
            if (ident == null)throw new UnauthorizedAccessException();
            role = ident.FindFirst(ClaimTypes.Role).Value;
            if (role == null) throw new UnauthorizedAccessException();
            // Vérifions que le user a bien le role envoyé via http
            bool isInRole = new IsInRoleUseCase(repositoryFactory).ExecuteAsync(email, role).Result; 
            if (!isInRole) throw new UnauthorizedAccessException();
            // Si tout est passé sans renvoyer d'exception, le user est authentifié et connecté
        }
    }
}
