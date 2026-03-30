using MA_Sys.API.Dto.AcademiasDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AcademiasController : Controller
    {
        private readonly AcademiaService _service;

        public AcademiasController(AcademiaService service)
        {
            _service = service;
        }

        private int GetAcademiaId()
        {
            var claim = User.FindFirst("AcademiaId");

            if (claim == null)
                throw new Exception("Token inválido");

            return int.Parse(claim.Value);
        }

        [HttpGet]
        public IActionResult List()
        {
            var academias = _service.List();
            return Ok(academias);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get([FromBody] AcademiaFiltroDto filtro)
        {
            var academiaId = GetAcademiaId();

            var academia = _service.Get(filtro, academiaId);

            return Ok(academia);
        }


        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add([FromBody] AcademiaCreateDto dto)
        {
            _service.Add(dto);

            return Ok("Academia criada om sucesso");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] AcademiaUpdateDto dto, int id)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Update(id, dto);

            return Ok();
        } 

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody]bool ativo)
        {
            var academiaId = GetAcademiaId();
            _service.UpdateStatus(id, academiaId, ativo);

            return NoContent();
        }

    }
}

