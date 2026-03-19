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
            #if DEBUG
                        return 1;
            #else
                var claim = User.FindFirst("AcademiaId");

                if (claim != null && int.TryParse(claim.Value, out int academiaId))
                    return academiaId;

                throw new UnauthorizedAccessException("AcademiaId não encontrado");
            #endif
        }

        /// <summary>
        /// Lista os alunos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var academiaId = GetAcademiaId();
            var alunos = _service.Listar(academiaId);

            return Ok(alunos);
        }


        /// <summary>
        /// Adiciona um novo aluno
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddAlunos([FromBody] AlunoDto dto)
        {
            var academiaId = GetAcademiaId();

            _service.Criar(dto, academiaId);

            return Ok();
        }
    }
}