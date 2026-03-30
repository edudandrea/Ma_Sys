using MA_Sys.API.Dto.Alunos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AlunosController : ControllerBase
    {
        private readonly AlunoService _service;

         public AlunosController(AlunoService service)
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
        public IActionResult Get([FromQuery] AlunoFiltroDto filtro)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");
            var alunos = _service.Get(filtro, academiaId);

            return Ok(alunos);
        }

        [HttpPost]        
        public async Task<IActionResult> Add([FromBody] AlunosCreateDto dto)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId);

            return Ok();
        } 

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] AlunoUpdateDto dto, int id)
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