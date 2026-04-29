using MA_Sys.API.Dto.PagamentosAcademias;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PagamentosAcademiasController : BaseController
    {
        private readonly PagamentoAcademiaService _service;

        public PagamentosAcademiasController(PagamentoAcademiaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? academiaId)
        {
            var (role, academiaIdUsuario, userId) = GetUserInfo();
            return Ok(_service.Listar(role, academiaIdUsuario, userId, academiaId));
        }

        [HttpPost]
        public IActionResult CriarCobranca([FromBody] PagamentoAcademiaCreateDto dto)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            var cobranca = _service.CriarCobranca(dto, GetUserRole(), GetUserId());
            return Ok(cobranca);
        }

        [HttpPatch("{id}/baixar")]
        public IActionResult Baixar(int id)
        {
            if (!RoleScope.IsAdmin(GetUserRole()) && !RoleScope.IsSuperAdmin(GetUserRole()))
                return Forbid();

            _service.BaixarPagamento(id, GetUserRole(), GetUserId());
            return NoContent();
        }

        [HttpPost("{id}/pix")]
        public async Task<IActionResult> GerarPix(int id)
        {
            try
            {
                var (role, academiaIdUsuario, userId) = GetUserInfo();
                var cobranca = await _service.GerarPagamentoPixAsync(id, role, academiaIdUsuario, userId);
                return Ok(cobranca);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cartao")]
        public async Task<IActionResult> PagarComCartao(int id, [FromBody] PagamentoAcademiaCartaoDto dto)
        {
            try
            {
                var (role, academiaIdUsuario, userId) = GetUserInfo();
                var cobranca = await _service.PagarComCartaoAsync(id, dto, role, academiaIdUsuario, userId);
                return Ok(cobranca);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/status-atualizado")]
        public IActionResult ConsultarStatusAtualizado(int id)
        {
            try
            {
                var (role, academiaIdUsuario, userId) = GetUserInfo();
                var cobranca = _service.AtualizarStatusPagamento(id, role, academiaIdUsuario, userId);
                return Ok(cobranca);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/pagar")]
        public async Task<IActionResult> PagarLegado(int id, [FromBody] PagamentoAcademiaCartaoDto dto)
        {
            var (role, academiaIdUsuario, userId) = GetUserInfo();
            var cobranca = await _service.PagarComCartaoAsync(id, dto, role, academiaIdUsuario, userId);
            return Ok(cobranca);
        }
    }
}
