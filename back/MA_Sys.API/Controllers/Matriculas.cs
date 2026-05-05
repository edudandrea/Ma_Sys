using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Dto.Matriculas;
using MA_Sys.API.Security;
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
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (role, academiaId, _) = GetUserInfo();
            var matriculas = _service.Get(role, filtro, academiaId);

            return Ok(matriculas);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] MatriculasCreateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            try
            {
                var (role, academiaId, _) = GetUserInfo();
                var matricula = await _service.Add(dto, academiaId, role);

                return Ok(matricula);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] MatriculasUpdateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            try
            {
                var (role, academiaId, _) = GetUserInfo();
                var matricula = _service.Update(id, dto, academiaId, role);
                return Ok(matricula);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            try
            {
                var (role, academiaId, _) = GetUserInfo();
                _service.Delete(id, academiaId, role);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
