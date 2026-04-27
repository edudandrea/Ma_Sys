using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Matriculas;
using MA_Sys.API.Models;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class MatriculaService
    {
        private readonly IMatriculaRepository _repo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IPlanosRepository _planoRepo;
        private readonly IFormaPagamentoRepository _formaPgtoRepo;
        private readonly IPagamentoRepository _pagamentoRepo;

        public MatriculaService(
            IMatriculaRepository repo,
            IAlunoRepository alunoRepo,
            IPlanosRepository planoRepo,
            IFormaPagamentoRepository formaPgtoRepo,
            IPagamentoRepository pagamentoRepo)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
            _planoRepo = planoRepo;
            _formaPgtoRepo = formaPgtoRepo;
            _pagamentoRepo = pagamentoRepo;
        }

        private List<Pagamentos> GerarPagamentos(Matricula matricula, Plano plano, FormaPagamento formaPgto, int formaPgtoId)
        {
            var lista = new List<Pagamentos>();
            var valorParcela = plano.Valor / formaPgto.Parcelas;

            for (var i = 0; i < formaPgto.Parcelas; i++)
            {
                var vencimento = matricula.DataInicio.AddDays(formaPgto.Dias * i);
                lista.Add(new Pagamentos
                {
                    AcademiaId = matricula.AcademiaId,
                    AlunoId = matricula.AlunoId,
                    PlanoId = matricula.PlanoId,
                    DataVencimento = vencimento,
                    Valor = valorParcela,
                    Status = "Pendente",
                    FormaPagamentoId = formaPgtoId
                });
            }

            return lista;
        }

        public List<MatriculasResponseDto> List(int academiaId)
        {
            return _repo.Query()
                .AsNoTracking()
                .Where(m => m.AcademiaId == academiaId)
                .Select(a => new MatriculasResponseDto
                {
                    Id = a.Id,
                    AlunoId = a.AlunoId,
                    PlanoId = a.PlanoId,
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    Status = a.Status
                })
                .ToList();
        }

        public List<MatriculasResponseDto> Get(string role, MatriculasFiltro filtro, int? academiaId)
        {
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) && !academiaId.HasValue)
            {
                throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");
            }

            var query =
                from matricula in _repo.Query().AsNoTracking()
                join aluno in _alunoRepo.Query().AsNoTracking() on matricula.AlunoId equals aluno.Id
                join plano in _planoRepo.Query().AsNoTracking() on matricula.PlanoId equals plano.Id
                join formaPagamento in _formaPgtoRepo.Query().AsNoTracking() on matricula.FormaPagamentoId equals formaPagamento.Id into formaPagamentoJoin
                from formaPagamento in formaPagamentoJoin.DefaultIfEmpty()
                select new MatriculasResponseDto
                {
                    Id = matricula.Id,
                    AlunoId = matricula.AlunoId,
                    AcademiaId = matricula.AcademiaId,
                    AlunoNome = aluno.Nome,
                    Email = aluno.Email,
                    Telefone = aluno.Telefone,
                    PlanoId = matricula.PlanoId,
                    PlanoNome = plano.Nome,
                    PlanoValor = plano.Valor,
                    FormaPagamentoNome = formaPagamento != null ? formaPagamento.Nome : string.Empty,
                    DataInicio = matricula.DataInicio,
                    DataFim = matricula.DataFim,
                    Status = matricula.Status
                };

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m => m.AcademiaId == academiaId);
            }

            if (!string.IsNullOrWhiteSpace(filtro?.AlunoNome))
            {
                var alunoNome = filtro.AlunoNome.Trim();
                query = query.Where(m => m.AlunoNome.Contains(alunoNome));
            }

            if (!string.IsNullOrWhiteSpace(filtro?.Status))
            {
                var status = filtro.Status.Trim();
                query = query.Where(m => m.Status == status);
            }

            return query.ToList();
        }

        public Task<Matricula> Add(MatriculasCreateDto dto, int? academiaId, string role)
        {
            var academiaDestino = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? dto.AcademiaId
                : academiaId ?? 0;

            if (academiaDestino <= 0)
            {
                throw new UnauthorizedAccessException("Academia invalida para criar matricula.");
            }

            var plano = _planoRepo.Query().FirstOrDefault(p => p.Id == dto.PlanoId && p.AcademiaId == academiaDestino);
            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado.");
            }

            var dataInicio = dto.DataInicio ?? DateTime.UtcNow;
            var matricula = new Matricula
            {
                AcademiaId = academiaDestino,
                AlunoId = dto.AlunoId,
                PlanoId = dto.PlanoId,
                FormaPagamentoId = dto.FormaPgtoId,
                DataInicio = dataInicio,
                DataFim = dataInicio.AddMonths(plano.DuracaoMeses),
                Status = "Ativa"
            };

            _repo.Add(matricula);
            _repo.Save();

            return Task.FromResult(matricula);
        }
    }
}
