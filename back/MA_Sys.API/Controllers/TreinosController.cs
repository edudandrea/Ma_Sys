using MA_Sys.API.Dto.Treinos;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TreinosController : BaseController
    {
        private readonly TreinoService _service;

        public TreinosController(TreinoService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            return Ok(_service.List(GetAcademiaId() ?? 0));
        }

        [HttpPost]
        public IActionResult Add([FromBody] TreinoCreateUpdateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            return Ok(_service.Add(dto, GetAcademiaId() ?? 0));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TreinoCreateUpdateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            return Ok(_service.Update(id, dto, GetAcademiaId() ?? 0));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            _service.Delete(id, GetAcademiaId() ?? 0);
            return NoContent();
        }
    }
}
