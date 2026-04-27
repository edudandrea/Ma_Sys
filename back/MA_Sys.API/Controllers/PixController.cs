using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Dto.PixDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PixController : Controller
    {
        private readonly PixService _pixService;

        public PixController(PixService pixService)
        {
            _pixService = pixService;
        }

        [HttpPost("pix")]
        public IActionResult GerarPix([FromBody] PixRequestDto dto)
        {
            var nome = dto.Nome.ToUpper();
            var cidade = dto.Cidade.ToUpper().Replace(" ", "");

            var payload = _pixService.GerarPixPayload(
                "00307910067",
                nome,
                cidade,
                dto.Valor
            );

            return Ok(new PixResponseDto
            {
                Payload = payload,
                Valor = dto.Valor
            });
        }

        [AllowAnonymous]
        [HttpPost("public")]
        public IActionResult GerarPixPublico([FromBody] PixRequestDto dto)
        {
            return GerarPix(dto);
        }

    }
}
