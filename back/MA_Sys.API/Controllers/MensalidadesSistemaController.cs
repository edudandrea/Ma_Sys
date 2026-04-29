using MA_Sys.API.Dto.MensalidadesSistema;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MensalidadesSistemaController : BaseController
    {
        private readonly MensalidadeSistemaService _service;

        public MensalidadesSistemaController(MensalidadeSistemaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(_service.Listar(GetUserRole(), GetUserId()));
        }

        [HttpPost]
        public IActionResult Adicionar([FromBody] MensalidadeSistemaCreateDto dto)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            _service.Adicionar(dto, GetUserRole(), GetUserId());
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromBody] MensalidadeSistemaUpdateDto dto)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            _service.Atualizar(id, dto, GetUserRole(), GetUserId());
            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            _service.AtualizarStatus(id, ativo, GetUserRole(), GetUserId());
            return NoContent();
        }
    }
}
