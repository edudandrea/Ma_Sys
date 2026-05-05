using MA_Sys.API.Controllers;
using MA_Sys.API.Dto.ProfessoresDto;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (role, academiaId, _) = GetUserInfo();
            var prof = _service.Get(role, filtro, academiaId);

            return Ok(prof);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProfessorCreateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (role, academiaId, _) = GetUserInfo();
            _service.Add(dto, academiaId, role);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ProfessorUpdateDto dto, int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (role, academiaId, _) = GetUserInfo();
            _service.Update(id, dto, role, academiaId);

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (_, academiaId, _) = GetUserInfo();
            _service.Delete(id, academiaId ?? 0);

            return NoContent();
        }
    }
}
