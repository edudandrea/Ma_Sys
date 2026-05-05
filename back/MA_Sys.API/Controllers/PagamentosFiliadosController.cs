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

        [AllowAnonymous]
        [HttpGet("public/{federacaoId}/config")]
        public IActionResult ConfigPublica(int federacaoId)
        {
            try
            {
                return Ok(_service.ObterConfiguracaoPublica(federacaoId));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("public/{federacaoId}/formas-pagamento")]
        public IActionResult FormasPagamentoPublicas(int federacaoId)
        {
            try
            {
                return Ok(_service.ListarFormasPagamentoPublicas(federacaoId));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("public/{federacaoId}/buscar")]
        public IActionResult BuscarFiliadoPublico(int federacaoId, [FromBody] PagamentoFiliadoBuscaPublicaDto busca)
        {
            try
            {
                return Ok(_service.BuscarFiliadoPublico(federacaoId, busca));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Filiado nao encontrado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("public/pix")]
        public async Task<IActionResult> GerarPixPublico([FromBody] PagamentoFiliadoPublicoDto dto)
        {
            try
            {
                return Ok(await _service.GerarPixPublicoAsync(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("public/cartao")]
        public async Task<IActionResult> PagarCartaoPublico([FromBody] PagamentoFiliadoCartaoPublicoDto dto)
        {
            try
            {
                var status = await _service.PagarCartaoPublicoAsync(dto);
                var mensagem = status.Status switch
                {
                    "Pago" => "Pagamento com cartao aprovado com sucesso.",
                    "Pendente" => "Pagamento pendente. Aguarde a confirmacao do emissor.",
                    "EmAnalise" => "Pagamento em analise. Retorne em instantes para consultar o status.",
                    _ => "Pagamento recusado pelo Mercado Pago."
                };

                return Ok(new
                {
                    pagamentoId = status.PagamentoId,
                    status = status.Status,
                    formaPagamentoNome = status.FormaPagamentoNome,
                    dataPagamento = status.DataPagamento,
                    mensagem
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("public/{federacaoId}/{pagamentoId}/status")]
        public IActionResult StatusPublico(int federacaoId, int pagamentoId)
        {
            try
            {
                return Ok(_service.AtualizarStatusPublico(federacaoId, pagamentoId));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
