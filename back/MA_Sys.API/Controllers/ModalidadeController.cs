using MA_Sys.API.Controllers;
using MA_Sys.API.Dto.ModalidadesDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ModalidadeController : BaseController
    {
        private readonly ModalidadeService _service;
        public ModalidadeController(ModalidadeService service)
        {
            _service = service;
        }
        
        [HttpGet]
        public IActionResult Get([FromQuery] ModalidadeFiltroDto filtro)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Role do usuário: {role}");
            Console.WriteLine($"Academia ID do usuário: {academiaId}");
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");         
            var modalidade = _service.Get(role,filtro, academiaId);

            return Ok(modalidade);
        }

        [HttpPost]
        
        public async Task<IActionResult> Add([FromBody] ModalidadeCreateDto dto)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId, role);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ModalidadeUpdateDto dto, int id)
        {
            var (role, academiaId) = GetUserInfo();
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
