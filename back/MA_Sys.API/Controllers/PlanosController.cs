using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Dto.Planos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PlanosController : BaseController
    {
        private readonly PlanosService _service;

        public PlanosController(PlanosService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult List()
        {
            var academias = _service.List();
            return Ok(academias);
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromQuery] PlanosFiltroDto filtro)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"ROLE: {role}");
            Console.WriteLine($"ACADEMIA ID: {academiaId}");

            var prof = _service.Get(role, filtro, academiaId);

            return Ok(prof);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PlanosCreateDto dto)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId, role);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] PlanosUpdateDto dto, int id)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Update(id, dto);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var (role, academiaId) = GetUserInfo();
            _service.UpdateStatus(id, academiaId, ativo);

            return NoContent();
        }

        
        
    }
}