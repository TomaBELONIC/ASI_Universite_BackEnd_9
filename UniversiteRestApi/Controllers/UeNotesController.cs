using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.NotesUseCases.Add;
using UniversiteDomain.UseCases.NotesUseCases.Get;
using UniversiteDomain.UseCases.NotesUseCases.Import;
using UniversiteDomain.UseCases.SecurityUseCases.Create;
using UniversiteDomain.UseCases.SecurityUseCases.Get;
using UniversiteEFDataProvider.Entities;
using Microsoft.AspNetCore.Http;

namespace UniversiteRestApi.Controllers

{
    [Route("api/ue/{idUe}/notes")]
    [ApiController]
    public class UeNotesController(IRepositoryFactory repositoryFactory) : ControllerBase
    {
        // GET api/<NoteController>/5
        [HttpGet("template-csv")]
        public async Task<ActionResult<UeDto>>  GetDonneesPourCetteUe(long idUe)
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
            
            GetNotesUeUseCase uc = new GetNotesUeUseCase(repositoryFactory);
            // On vérifie si l'utilisateur connecté a le droit d'accéder à la ressource
            if (!uc.IsAuthorized(role)) return Unauthorized();
            Ue? ue;
            try
            {
                ue = await uc.ExecuteAsync(idUe);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }
           
            if (ue == null) return NotFound();
            
            return new UeDto().ToDto(ue);
        }
        
        // POST api/<NoteController>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImporterDonneesPourCetteUe(long idUe, IFormFile fichierImporte)
        {
            if (fichierImporte == null || fichierImporte.Length == 0) return BadRequest("Fichier manquant");
            
            ImportNotesUeUseCase importNotesUeUseCase = new ImportNotesUeUseCase(repositoryFactory);
            CreateUniversiteUserUseCase createUserUc = new CreateUniversiteUserUseCase(repositoryFactory);
        
            string role="";
            string email="";
            IUniversiteUser user = null;
            CheckSecu(out role, out email, out user);
            if (!importNotesUeUseCase.IsAuthorized(role) || !createUserUc.IsAuthorized(role)) return Unauthorized();
        
        
            try
            {
                Ue ue = await repositoryFactory.UeRepository().FindAsync(idUe);
                var donneesAMettreEnBase = repositoryFactory.UeRepository().LireLeFichierNotesPourCetteUe(ue, fichierImporte);
                var donnees = await importNotesUeUseCase.ExecuteAsync2(idUe, donneesAMettreEnBase);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(e), e.Message);
                return ValidationProblem();
            }
            
            return Ok(new { Resultat = "Les notes pour cettes UE ont bien été enregistré dans la Base de Données" });
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
