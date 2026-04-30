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

        [HttpPost("pix")]
        public async Task<IActionResult> GerarPixMatricula([FromBody] PagamentoPixCreateDto dto)
        {
            try
            {
                if (GetAcademiaId() is not int academiaId || academiaId <= 0)
                {
                    return Unauthorized(new { message = "Usuario sem vinculo com academia." });
                }

                var response = await _service.GerarPagamentoPixAsync(dto, academiaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("manual")]
        public IActionResult RegistrarPagamentoManual([FromBody] PagamentoManualCreateDto dto)
        {
            try
            {
                if (GetAcademiaId() is not int academiaId || academiaId <= 0)
                {
                    return Unauthorized(new { message = "Usuario sem vinculo com academia." });
                }

                var pagamento = _service.RegistrarPagamentoManual(dto, academiaId);
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

        [HttpPost("cartao")]
        public async Task<IActionResult> PagarCartaoMatricula([FromBody] PagamentoCartaoPublicoDto dto)
        {
            try
            {
                if (GetAcademiaId() is not int academiaId || academiaId <= 0)
                {
                    return Unauthorized(new { message = "Usuario sem vinculo com academia." });
                }

                var pagamento = await _service.ProcessarPagamentoCartaoPublico(dto, academiaId);

                var mensagem = pagamento.Status switch
                {
                    "Pago" => "Pagamento com cartao aprovado com sucesso.",
                    "Pendente" => "Pagamento pendente. Aguarde a confirmacao do emissor.",
                    "EmAnalise" => "Pagamento em analise. Retorne em instantes para consultar o status.",
                    _ => "Pagamento recusado."
                };

                return Ok(new
                {
                    pagamentoId = pagamento.Id,
                    status = pagamento.Status,
                    mensagem
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("public/pix")]
        public async Task<IActionResult> GerarPixPublico([FromBody] PagamentoPixPublicoDto dto)
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

                var response = await _service.GerarPagamentoPixPublicoAsync(dto, academiaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("public/cartao")]
        public async Task<IActionResult> PagarCartaoPublico([FromBody] PagamentoCartaoPublicoDto dto)
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

                var pagamento = await _service.ProcessarPagamentoCartaoPublico(dto, academiaId);

                var mensagem = pagamento.Status switch
                {
                    "Pago" => "Pagamento com cartao aprovado com sucesso.",
                    "Pendente" => "Pagamento pendente. Aguarde a confirmacao do emissor.",
                    "EmAnalise" => "Pagamento em analise. Retorne em instantes para consultar o status.",
                    _ => "Pagamento recusado."
                };

                return Ok(new
                {
                    pagamentoId = pagamento.Id,
                    status = pagamento.Status,
                    mensagem
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("public/{pagamentoId}/status")]
        public IActionResult ConsultarStatusPublico(int pagamentoId, [FromQuery] string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return BadRequest(new { message = "Slug da academia nao informado." });
                }

                var academiaId = _academiaRepository.Query()
                    .Where(a => a.Slug == slug)
                    .Select(a => a.Id)
                    .FirstOrDefault();

                if (academiaId <= 0)
                {
                    return NotFound(new { message = "Academia nao encontrada." });
                }

                var pagamento = _service.ObterPagamentoPorId(pagamentoId, academiaId);
                if (pagamento == null)
                {
                    return NotFound(new { message = "Pagamento nao encontrado." });
                }

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

        [HttpGet("{pagamentoId}/status-atualizado")]
        public IActionResult ConsultarStatusAtualizado(int pagamentoId)
        {
            try
            {
                if (GetAcademiaId() is not int academiaId || academiaId <= 0)
                {
                    return Unauthorized(new { message = "Usuario sem vinculo com academia." });
                }

                var pagamento = _service.AtualizarStatusPagamento(pagamentoId, academiaId);
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
    }
}
