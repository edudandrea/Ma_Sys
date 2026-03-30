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
    public class ProfessoresController : Controller
    {
        private readonly ProfessorService _service;

         public ProfessoresController(ProfessorService service)
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
        public IActionResult List(int academiaId)
        {
            var prof = _service.List(academiaId);
            return Ok(prof);
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromQuery] ProfessorFiltroDto filtro)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");
            var prof = _service.Get(filtro, academiaId);

            return Ok(prof);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProfessorCreateDto dto)
        {
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId);

            return Ok();
        }

    }
}