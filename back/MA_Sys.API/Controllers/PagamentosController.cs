using MA_Sys.API.Dto.Pagamentos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MA_Sys.API.Data.Repository.interfaces;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PagamentosController : BaseController
    {
        private readonly PagamentoService _service;
        private readonly IAcademiaRepository _academiaRepository;

        public PagamentosController(PagamentoService service, IAcademiaRepository academiaRepository)
        {
            _service = service;
            _academiaRepository = academiaRepository;
        }

        [HttpPost]
        public async Task<IActionResult> RegistraPagamento([FromBody] PagamentoRegistroDto dto)
        {
            try
            {
                var pagamento = await _service.RegistraPagamento(dto);

                // 🔥 retorno importante
                return Ok(new
                {
                    pagamentoId = pagamento.Id,
                    status = pagamento.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public IActionResult WebhookPix([FromBody] dynamic payload)
        {
            _service.ProcessarWebhook(payload);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("public/cartao")]
        public IActionResult PagarCartaoPublico([FromBody] PagamentoCartaoPublicoDto dto)
        {
            try
            {
                var academiaId = _academiaRepository.Query()
                    .Where(a => a.Slug == dto.Slug)
                    .Select(a => a.Id)
                    .FirstOrDefault();

                if (academiaId <= 0)
                {
                    return NotFound(new { message = "Academia nao encontrada." });
                }

                var pagamento = _service.ProcessarPagamentoCartaoPublico(dto, academiaId);

                return Ok(new
                {
                    pagamentoId = pagamento.Id,
                    status = pagamento.Status,
                    mensagem = "Pagamento com cartao aprovado com sucesso."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
