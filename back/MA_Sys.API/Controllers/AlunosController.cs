using MA_Sys.API.Dto.Alunos;
using MA_Sys.API.Services;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AlunosController : BaseController
    {
        private readonly AlunoService _service;

        public AlunosController(AlunoService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] AlunoFiltroDto filtro)
        {
            var (role, academiaId) = GetUserInfo();
            Console.WriteLine($"ACADEMIA LOGADA: {academiaId}");
            var alunos = _service.Get(role, filtro, academiaId);

            return Ok(alunos);
        }

        [HttpGet("{slug}")]
        public IActionResult BuscarAluno(string slug, [FromQuery] string cpf, [FromQuery] string email)
        {
            var academiaId = ObterAcademiaIdPeloSlug(slug);

            var matricula = _service.BuscarMatriculaPorCpfEmail(cpf, email, academiaId);

            if (matricula == null)
                return NotFound();

            return Ok(new
            {
                alunoId = matricula.Aluno.Id,
                matriculaId = matricula.Id,
                planoId = matricula.PlanoId,
                academiaId = matricula.AcademiaId,
                formaPagamentoId = matricula.FormaPagamentoId,
                nome = matricula.Aluno.Nome,
                cpf = matricula.Aluno.CPF,
                email = matricula.Aluno.Email,
                endereco = matricula.Aluno.Endereco,
                bairro = matricula.Aluno.Bairro,
                cidade = matricula.Aluno.Cidade,
                redeSocial = matricula.Aluno.RedeSocial,
                telefone = matricula.Aluno.Telefone,
                graduacao = matricula.Aluno.Graduacao,
                dataNascimento = matricula.Aluno.DataNascimento.ToString("yyyy-MM-dd"),
                dataCadastro = matricula.Aluno.DataCadastro.ToString("yyyy-MM-dd"),
                obs = matricula.Aluno.Obs,
                plano = matricula.Plano.Nome,
                valor = matricula.Plano.Valor
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AlunosCreateDto dto)
        {

            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Add(dto, academiaId);

            return Ok();
        }

        [HttpPost("{slug}")]
        public IActionResult Cadastrar(string slug, [FromBody] AlunosCreateDto dto)
        {
            var academiaId = ObterAcademiaIdPeloSlug(slug);

            _service.Add(dto, academiaId);

            return Ok(new
            {
                sucesso = true,
                mensagem = "Aluno cadastrado com sucesso"
            }

            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] AlunoUpdateDto dto, int id)
        {
            var role = GetUserRole();
            var academiaId = GetAcademiaId();
            Console.WriteLine($"Academia ID: {academiaId}");

            _service.Update(id, dto);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var (role, academiaId) = GetUserInfo();
            _service.UpdateStatus(id, academiaId, ativo);

            return NoContent();
        }

    }
}
