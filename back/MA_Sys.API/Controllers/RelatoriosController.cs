using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RelatoriosController : BaseController
    {
        private readonly RelatorioService _service;

        public RelatoriosController(RelatorioService service)
        {
            _service = service;
        }

        [HttpGet("resumo")]
        public IActionResult Resumo([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            var (role, academiaId, userId) = GetUserInfo();
            return Ok(_service.ObterResumo(role, academiaId, userId, inicio, fim));
        }
    }
}
