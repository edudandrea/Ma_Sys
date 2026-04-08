using MA_Sys.API.Controllers;
using MA_Sys.API.Dto.ProfessoresDto;
using MA_Sys.API.Services;
using MA_SYS.Api.Data;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProfessoresController : BaseController
    {
        private readonly ProfessorService _service;

        public ProfessoresController(ProfessorService service)
        {
            _service = service;
        }        
        
        [HttpGet]
        public IActionResult Get([FromQuery] ProfessorFiltroDto filtro)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"ROLE: {role}");
            Console.WriteLine($"ACADEMIA ID: {academiaId}");

            var prof = _service.Get(role, filtro, academiaId);

            return Ok(prof);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProfessorCreateDto dto)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId, role);

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var (role, academiaId) = GetUserInfo();
            _service.Delete(id, academiaId ?? 0);

            return NoContent();
        }

    }
}