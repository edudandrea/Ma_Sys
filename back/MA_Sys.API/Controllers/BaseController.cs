using System.Security.Claims;
using MA_Sys.API.Data.Repository.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [Route("[controller]")]
    public class BaseController : Controller
    {
        protected string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        protected int? GetAcademiaId()
        {
            var claim = User.FindFirst("AcademiaId");
            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
            {
                return null;
            }

            return int.TryParse(claim.Value, out var academiaId) ? academiaId : null;
        }

        protected (string role, int? academiaId) GetUserInfo()
        {
            return (GetUserRole(), GetAcademiaId());
        }

        protected int ObterAcademiaIdPeloSlug(string slug)
        {
            var academiaRepo = HttpContext.RequestServices
                .GetService(typeof(IAcademiaRepository)) as IAcademiaRepository;

            if (academiaRepo == null)
            {
                throw new InvalidOperationException("Repositorio de academia nao disponivel.");
            }

            var academia = academiaRepo.Query()
                .FirstOrDefault(a => a.Slug == slug);

            if (academia == null)
            {
                throw new InvalidOperationException("Academia nao encontrada.");
            }

            return academia.Id;
        }
    }
}
