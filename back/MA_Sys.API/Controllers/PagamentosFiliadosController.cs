using MA_Sys.API.Dto.PagamentosFiliados;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PagamentosFiliadosController : BaseController
    {
        private readonly PagamentoFiliadoService _service;

        public PagamentosFiliadosController(PagamentoFiliadoService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? filiadoId)
        {
            var (role, academiaId, userId) = GetUserInfo();
            return Ok(_service.Listar(role, academiaId, userId, filiadoId));
        }

        [HttpPost]
        public IActionResult Criar([FromBody] PagamentoFiliadoCreateDto dto)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) &&
                !RoleScope.IsSuperAdmin(GetUserRole()) &&
                !RoleScope.IsFederacao(GetUserRole()))
            {
                return Forbid();
            }

            return Ok(_service.CriarCobranca(dto, GetUserRole(), GetUserId()));
        }

        [HttpPatch("{id}/baixar")]
        public IActionResult Baixar(int id)
        {
            _service.Baixar(id, GetUserRole(), GetUserId());
            return NoContent();
        }

        [HttpPost("{id}/pix")]
        public async Task<IActionResult> GerarPix(int id)
        {
            try
            {
                var (role, academiaId, userId) = GetUserInfo();
                return Ok(await _service.GerarPixAsync(id, role, academiaId, userId));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
