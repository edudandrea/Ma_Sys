using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Dto.FormaPagamentos;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MA_Sys.API.Data.Repository.interfaces;

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
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Role do usuário: {role}");
            Console.WriteLine($"Academia ID do usuário: {academiaId}");
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");
            var formaPagamento = _service.Get(role, filtro, academiaId);

            return Ok(formaPagamento);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] FormaPagamentoCreateDto dto)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId, role);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] FormaPagamentoUpdateDto dto, int id)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Update(id, dto);

            return Ok();
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
