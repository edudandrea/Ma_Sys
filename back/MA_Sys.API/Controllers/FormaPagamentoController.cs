using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.FormaPagamentos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FormaPagamentoController : BaseController
    {
        private readonly FormaPagamentoService _service;
        private readonly IAcademiaRepository _academiaRepository;

        public FormaPagamentoController(FormaPagamentoService service, IAcademiaRepository academiaRepository)
        {
            _service = service;
            _academiaRepository = academiaRepository;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] FormaPagamentoFiltroDto filtro)
        {
            var (role, academiaId, userId) = GetUserInfo();
            var formaPagamento = _service.Get(role, filtro, academiaId, userId);

            return Ok(formaPagamento);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] FormaPagamentoCreateDto dto)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Add(dto, academiaId, role, userId);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] FormaPagamentoUpdateDto dto, int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Update(id, dto, role, academiaId, userId);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.UpdateStatus(id, role, academiaId, userId, ativo);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("public/{slug}")]
        public IActionResult GetPublico(string slug)
        {
            var academiaId = _academiaRepository.Query()
                .Where(a => a.Slug == slug)
                .Select(a => a.Id)
                .FirstOrDefault();

            if (academiaId <= 0)
            {
                return NotFound(new { message = "Academia nao encontrada." });
            }

            var formasPagamento = _service.List(academiaId)
                .Where(f => f.Ativo)
                .ToList();

            return Ok(formasPagamento);
        }
    }
}
