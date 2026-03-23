using System.Windows.Markup;
using MA_Sys.API.Dto.Alunos;
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
    [Route("api/[controller]")]
    public class AlunosController : Controller
    {
        private readonly AlunoService _service;

        public AlunosController(AlunoService service)
        {
            _service = service;
        }

        private int GetAcademiaId()
        {           
                var claim = User.FindFirst("AcademiaId");

                if (claim != null )
                throw new Exception("Token inválido");

                return int.Parse(claim.Value);            
        }

        /// <summary>
        /// Lista os alunos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get([FromQuery] AlunoFiltroDto filtro)
        {
            var academiaId = GetAcademiaId();
            var alunos = _service.Buscar(filtro, academiaId);

            return Ok(alunos);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var academiaId = GetAcademiaId();
            var aluno = _service.ObterPorId(id, academiaId);

            if(aluno == null) return NotFound();
            
            return Ok(aluno);
        }    
        /// <summary>
        /// Adiciona um novo aluno
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddAlunos([FromBody] AlunosCreateDto dto)
        {
            var academiaId = GetAcademiaId();

            _service.Criar(dto, academiaId);

            return Ok();
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAlunos(int id, [FromBody] AlunoUpdateDto dto)
        {
            var academiaId = GetAcademiaId();

            _service.Atualizar(id, dto, academiaId);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody]bool ativo)
        {
            var academiaId = GetAcademiaId();
            _service.AlterarStatus(id, academiaId, ativo);

            return NoContent();
        }
    }
}