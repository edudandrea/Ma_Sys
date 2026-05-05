using MA_Sys.API.Dto.Alunos;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using MA_SYS.Api.Dto;
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
        private readonly IPagamentoRepository _pagamentoRepository;

        public AlunosController(AlunoService service, IPagamentoRepository pagamentoRepository)
        {
            _service = service;
            _pagamentoRepository = pagamentoRepository;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] AlunoFiltroDto filtro)
        {
            var (role, academiaId, _) = GetUserInfo();
            if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
                return Forbid();

            var alunos = _service.Get(role, filtro, academiaId);

            return Ok(alunos);
        }

        [AllowAnonymous]
        [HttpPost("public/{slug}/buscar")]
        public IActionResult BuscarAlunoPublico(string slug, [FromBody] AlunoBuscaPublicaDto busca)
        {
            var academiaId = ObterAcademiaIdPeloSlug(slug);
            var matricula = _service.BuscarMatriculaPorCpfEmail(busca.Cpf, busca.Email, academiaId);

            if (matricula == null)
                return NotFound();

            var pagamentoAtual = _pagamentoRepository.Query()
                .Where(p => p.MatriculaId == matricula.Id)
                .OrderByDescending(p => p.DataVencimento)
                .FirstOrDefault();

            var hoje = DateTime.UtcNow.Date;
            var diaBase = matricula.DataInicio.Day;
            var diaVencimentoMesAtual = Math.Min(diaBase, DateTime.DaysInMonth(hoje.Year, hoje.Month));
            var vencimentoAtual = new DateTime(hoje.Year, hoje.Month, diaVencimentoMesAtual);

            var dataReferencia = pagamentoAtual?.Status == "Pago" && pagamentoAtual.DataVencimento.Date > vencimentoAtual
                ? pagamentoAtual.DataVencimento
                : vencimentoAtual;

            var diasParaVencimento = (dataReferencia.Date - hoje).Days;
            var mensalidadeStatus = pagamentoAtual?.Status == "Pago" && pagamentoAtual.DataVencimento.Date >= vencimentoAtual
                ? "Pago"
                : diasParaVencimento < 0
                    ? "Em atraso"
                    : diasParaVencimento <= 5
                        ? "Pendente"
                        : "Em dia";
            var alertaVencimento = diasParaVencimento <= 5;

            return Ok(new
            {
                alunoId = matricula.Aluno.Id,
                matriculaId = matricula.Id,
                planoId = matricula.PlanoId,
                nome = matricula.Aluno.Nome,
                endereco = matricula.Aluno.Endereco,
                bairro = matricula.Aluno.Bairro,
                cidade = matricula.Aluno.Cidade,
                estado = matricula.Aluno.Estado,
                cep = matricula.Aluno.CEP,
                redeSocial = matricula.Aluno.RedeSocial,
                telefone = matricula.Aluno.Telefone,
                graduacao = matricula.Aluno.Graduacao,
                dataNascimento = matricula.Aluno.DataNascimento.ToString("yyyy-MM-dd"),
                dataCadastro = matricula.Aluno.DataCadastro.ToString("yyyy-MM-dd"),
                obs = matricula.Aluno.Obs,
                plano = matricula.Plano.Nome,
                valor = matricula.Plano.Valor,
                mensalidadeStatus,
                dataVencimentoMensalidade = dataReferencia.ToString("yyyy-MM-dd"),
                diasParaVencimento,
                alertaVencimento
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AlunosCreateDto dto)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var academiaId = GetAcademiaId();
            var aluno = _service.Add(dto, academiaId);
            return Ok(aluno);
        }

        [HttpPost("{slug}")]
        public IActionResult Cadastrar(string slug, [FromBody] AlunosCreateDto dto)
        {
            var academiaId = ObterAcademiaIdPeloSlug(slug);

            var aluno = _service.Add(dto, academiaId);
            return Ok(aluno);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] AlunoUpdateDto dto, int id)
        {
            if (RoleScope.IsAdmin(GetUserRole()) || RoleScope.IsFederacao(GetUserRole()))
                return Forbid();

            var role = GetUserRole();
            var academiaId = GetAcademiaId();

            var aluno = _service.Update(id, dto, role, academiaId);
            return Ok(aluno);
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
    }
}
