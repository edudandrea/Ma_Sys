using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Alunos;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class AlunoService
    {
        private readonly IAlunoRepository _repo;
        private readonly IMatriculaRepository _matriculaRepo;
        private readonly MensalidadeStatusService _mensalidadeStatusService;

        public AlunoService(
            IAlunoRepository repo,
            IMatriculaRepository matriculaRepo,
            MensalidadeStatusService mensalidadeStatusService)
        {
            _repo = repo;
            _matriculaRepo = matriculaRepo;
            _mensalidadeStatusService = mensalidadeStatusService;
        }

        public List<AlunoResponseDto> List(string role, int? academiaId)
        {
            var alunos = _repo.Query();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                alunos = alunos.Where(a => a.AcademiaId == academiaId);
            }

            return alunos.Select(a => new AlunoResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                CPF = a.CPF,
                Telefone = a.Telefone,
                Ativo = a.Ativo
            }).ToList();
        }

        public List<AlunoResponseDto> Get(string role, AlunoFiltroDto filtro, int? academiaId)
        {
            var query = _repo.Query();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            if (filtro.Id.HasValue)
                query = query.Where(a => a.Id == filtro.Id);

            if (!string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(a => a.Nome.Contains(filtro.Nome));

            if (!string.IsNullOrEmpty(filtro.CPF))
                query = query.Where(a => a.CPF.Contains(filtro.CPF));

            var alunos = query.Select(a => new AlunoResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                CPF = a.CPF,
                Endereco = a.Endereco,
                Bairro = a.Bairro,
                Cidade = a.Cidade,
                Estado = a.Estado,
                CEP = a.CEP,
                Email = a.Email,
                RedeSocial = a.RedeSocial,
                ModalidadeId = a.ModalidadeId,
                DataNascimento = a.DataNascimento,
                DataCadastro = a.DataCadastro,
                Telefone = a.Telefone,
                Graduacao = a.Graduacao,
                Ativo = a.Ativo,
                AcademiaId = a.AcademiaId,
                PlanoId = a.PlanoId,
                Obs = a.Obs
            }).ToList();

            foreach (var aluno in alunos)
            {
                var mensalidade = _mensalidadeStatusService.CalcularPorAluno(aluno.Id);
                aluno.MensalidadeStatus = mensalidade.Status;
                aluno.DataVencimentoMensalidade = mensalidade.DataVencimento;
                aluno.DiasParaVencimento = mensalidade.DiasParaVencimento;
            }

            return alunos;
        }

        public AlunoDto? GetById(int id, int academiaId)
        {
            return _repo.Query()
                .Where(a => a.Id == id && a.AcademiaId == academiaId)
                .Select(a => new AlunoDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    CPF = a.CPF,
                    Telefone = a.Telefone,
                    Email = a.Email,
                    Endereco = a.Endereco,
                    Cidade = a.Cidade,
                    Estado = a.Estado,
                    CEP = a.CEP,
                    ModalidadeId = a.ModalidadeId,
                    Graduacao = a.Graduacao,
                    DataCadastro = a.DataCadastro,
                    DataNascimento = a.DataNascimento,
                    Ativo = a.Ativo,
                    Obs = a.Obs
                })
                .FirstOrDefault();
        }

        public Aluno? BuscarPorCpfEmail(string cpf, string email, int academiaId)
        {
            var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            var emailLimpo = email.Trim().ToLowerInvariant();

            return _repo.Query()
                .FirstOrDefault(a =>
                    a.CPF.Replace(".", "").Replace("-", "") == cpfLimpo &&
                    a.Email.ToLower() == emailLimpo &&
                    a.AcademiaId == academiaId
                );
        }

        public Matricula? BuscarMatriculaPorCpfEmail(string cpf, string email, int academiaId)
        {
            var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            var emailLimpo = email.Trim().ToLowerInvariant();

            return _matriculaRepo.Query()
                .Include(m => m.Aluno)
                .Include(m => m.Plano)
                .FirstOrDefault(m =>
                    m.Aluno.CPF.Replace(".", "").Replace("-", "") == cpfLimpo &&
                    m.Aluno.Email.ToLower() == emailLimpo &&
                    m.AcademiaId == academiaId
                );
        }

        public List<AlunoResponseDto> GetByCpfEmail(string cpf, string email, int academiaId)
        {
            var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            var emailLimpo = email.Trim().ToLowerInvariant();

            var query = _repo.Query()
                .Where(a =>
                    a.CPF.Replace(".", "").Replace("-", "") == cpfLimpo &&
                    a.Email.ToLower() == emailLimpo &&
                    a.AcademiaId == academiaId
                );

            return query.Select(a => new AlunoResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                CPF = a.CPF,
                Telefone = a.Telefone,
                Ativo = a.Ativo
            }).ToList();
        }

        public AlunoResponseDto Add(AlunosCreateDto dto, int? academiaId)
        {
            var aluno = new Aluno
            {
                Nome = dto.Nome,
                CPF = dto.CPF,
                ModalidadeId = dto.ModalidadeId,
                Telefone = dto.Telefone,
                Email = dto.Email,
                PlanoId = dto.PlanoId,
                DataNascimento = dto.DataNascimento,
                AcademiaId = academiaId ?? 0,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _repo.Add(aluno);
            _repo.Save();
            return Get(role: "Professor", new AlunoFiltroDto { Id = aluno.Id }, academiaId).First();
        }

        public AlunoResponseDto Update(int id, AlunoUpdateDto dto, string role, int? academiaId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var aluno = query.FirstOrDefault();

            if (aluno == null)
                throw new Exception("Aluno nao encontrado");

            aluno.Nome = dto.Nome?.Trim();
            aluno.CPF = dto.CPF?.Trim();
            aluno.Telefone = dto.Telefone;
            aluno.Endereco = dto.Endereco;
            aluno.Cidade = dto.Cidade;
            aluno.CEP = dto.CEP;
            aluno.Estado = dto.Estado;
            aluno.RedeSocial = dto.RedeSocial;
            aluno.Email = dto.Email;
            aluno.ModalidadeId = dto.ModalidadeId;
            aluno.Graduacao = dto.Graduacao;
            aluno.DataNascimento = dto.DataNascimento;
            aluno.PlanoId = dto.PlanoId;
            aluno.Obs = dto.Obs;

            _repo.Save();
            return Get(role, new AlunoFiltroDto { Id = aluno.Id }, academiaId).First();
        }

        public void UpdateStatus(int id, int? academiaId, bool ativo)
        {
            var aluno = _repo.GetById(id, academiaId ?? 0);

            if (aluno == null)
                throw new Exception("Aluno nao encontrado");

            aluno.Ativo = ativo;

            _repo.Update(aluno);
            _repo.Save();
        }
    }
}
