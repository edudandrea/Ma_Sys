using MA_Sys.API.Dto.ModalidadesDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ModalidadeController : ControllerBase
    {
        private readonly ModalidadeService _service;
        public ModalidadeController(ModalidadeService service)
        {
            _service = service;
        }

        private int GetAcademiaId()
        {           
                var claim = User.FindFirst("AcademiaId");

                if (claim == null )
                throw new Exception("Token inválido");

                return int.Parse(claim.Value);            
        }

        [HttpGet]
        public IActionResult Get([FromQuery] ModalidadeFiltroDto filtro)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");
            var modalidade = _service.Get(filtro, academiaId);

            return Ok(modalidade);
        }

        [HttpPost]
        
        public async Task<IActionResult> Add([FromBody] ModalidadeCreateDto dto)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ModalidadeUpdateDto dto, int id)
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
