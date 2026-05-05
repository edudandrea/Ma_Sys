using MA_Sys.API.Controllers;
using MA_Sys.API.Dto.ModalidadesDto;
using MA_Sys.API.Security;
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
            if (RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (role, academiaId, _) = GetUserInfo();
            var modalidade = _service.Get(role, filtro, academiaId);

            return Ok(modalidade);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ModalidadeCreateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (role, academiaId, _) = GetUserInfo();
            _service.Add(dto, academiaId, role);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ModalidadeUpdateDto dto, int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            _service.Update(id, dto);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (_, academiaId, _) = GetUserInfo();
            _service.UpdateStatus(id, academiaId, ativo);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var (_, academiaId, _) = GetUserInfo();
            _service.Delete(id, academiaId);

            return NoContent();
        }
    }
}
