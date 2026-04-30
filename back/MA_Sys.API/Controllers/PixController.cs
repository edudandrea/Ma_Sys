using MA_Sys.API.Dto.PixDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PixController : BaseController
    {
        [HttpPost("pix")]
        public IActionResult GerarPix([FromBody] PixRequestDto dto)
        {
            return StatusCode(StatusCodes.Status410Gone, new
            {
                message = "A geracao manual de PIX por chave foi removida. Use a integracao do Mercado Pago para gerar cobrancas PIX."
            });
        }

        [AllowAnonymous]
        [HttpPost("public")]
        public IActionResult GerarPixPublico([FromBody] PixRequestDto dto)
        {
            return StatusCode(StatusCodes.Status410Gone, new
            {
                message = "A geracao manual de PIX por chave foi removida. Use a integracao do Mercado Pago para gerar cobrancas PIX."
            });
        }

    }
}
