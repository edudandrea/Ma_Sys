using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Dto.Matriculas;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class Matriculas : BaseController
    {
       private readonly MatriculaService _service;

        public Matriculas(MatriculaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] MatriculasFiltro filtro)
        {
            var (role, academiaId) = GetUserInfo();
            var matriculas = _service.Get(role, filtro, academiaId);

            return Ok(matriculas);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MatriculasCreateDto dto)
        {
            var (role, academiaId) = GetUserInfo();
            await _service.Add(dto, academiaId, role);

            return Ok();
        }

    }
}
