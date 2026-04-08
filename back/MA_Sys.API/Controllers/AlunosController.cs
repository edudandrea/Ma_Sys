using MA_Sys.API.Dto.Alunos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AlunosController : BaseController
    {
        private readonly AlunoService _service;

         public AlunosController(AlunoService service)
        {
            _service = service;
        }         

        [HttpGet]
        public IActionResult Get([FromQuery] AlunoFiltroDto filtro)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");
            var alunos = _service.Get(role, filtro, academiaId);

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
            var role = GetUserRole();
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Update(id, dto);

            return Ok();
        } 

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody]bool ativo)
        {
            var (role, academiaId) = GetUserInfo();
            _service.UpdateStatus(id, academiaId, ativo);

            return NoContent();
        }
        
    }
}