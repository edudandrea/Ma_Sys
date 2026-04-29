using MA_Sys.API.Dto.Turmas;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TurmasController : BaseController
    {
        private readonly TurmaService _service;

        public TurmasController(TurmaService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (RoleScope.IsAdmin(GetUserRole()))
                return Forbid();

            var academiaId = GetAcademiaId();
            return Ok(_service.List(academiaId ?? 0));
        }

        [HttpPost]
        public IActionResult Add([FromBody] TurmaCreateUpdateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()))
                return Forbid();

            var academiaId = GetAcademiaId();
            return Ok(_service.Add(dto, academiaId ?? 0));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TurmaCreateUpdateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()))
                return Forbid();

            var academiaId = GetAcademiaId();
            return Ok(_service.Update(id, dto, academiaId ?? 0));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()))
                return Forbid();

            var academiaId = GetAcademiaId();
            _service.Delete(id, academiaId ?? 0);
            return NoContent();
        }
    }
}
