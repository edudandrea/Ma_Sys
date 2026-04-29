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
        private readonly MensalidadeStatusService _mensalidadeStatusService;

        public MatriculaService(
            IMatriculaRepository repo,
            IAlunoRepository alunoRepo,
            IPlanosRepository planoRepo,
            IFormaPagamentoRepository formaPgtoRepo,
            IPagamentoRepository pagamentoRepo,
            MensalidadeStatusService mensalidadeStatusService)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
            _planoRepo = planoRepo;
            _formaPgtoRepo = formaPgtoRepo;
            _pagamentoRepo = pagamentoRepo;
            _mensalidadeStatusService = mensalidadeStatusService;
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
            return Get("Professor", new MatriculasFiltro(), academiaId);
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
                    FormaPagamentoId = matricula.FormaPagamentoId,
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

            var matriculas = query.ToList();
            var pagamentosPorMatricula = _pagamentoRepo.Query()
                .Where(p => matriculas.Select(m => m.Id).Contains(p.MatriculaId))
                .AsEnumerable()
                .GroupBy(p => p.MatriculaId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            foreach (var matricula in matriculas)
            {
                var entity = new Matricula
                {
                    Id = matricula.Id,
                    AcademiaId = matricula.AcademiaId,
                    AlunoId = matricula.AlunoId,
                    PlanoId = matricula.PlanoId,
                    FormaPagamentoId = matricula.FormaPagamentoId,
                    DataInicio = matricula.DataInicio,
                    DataFim = matricula.DataFim,
                    Status = matricula.Status
                };

                var mensalidade = _mensalidadeStatusService.Calcular(
                    entity,
                    pagamentosPorMatricula.TryGetValue(matricula.Id, out var pagamentos)
                        ? pagamentos
                        : []);

                matricula.MensalidadeStatus = mensalidade.Status;
                matricula.DataVencimentoMensalidade = mensalidade.DataVencimento;
                matricula.DiasParaVencimento = mensalidade.DiasParaVencimento;
            }

            return matriculas;
        }

        public Task<MatriculasResponseDto> Add(MatriculasCreateDto dto, int? academiaId, string role)
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

            return Task.FromResult(Get(role, new MatriculasFiltro(), academiaDestino).First(m => m.Id == matricula.Id));
        }

        public MatriculasResponseDto Update(int id, MatriculasUpdateDto dto, int? academiaId, string role)
        {
            var matricula = _repo.Query().FirstOrDefault(m => m.Id == id);
            if (matricula == null)
            {
                throw new InvalidOperationException("Matricula nao encontrada.");
            }

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) && matricula.AcademiaId != academiaId)
            {
                throw new UnauthorizedAccessException("Usuario sem permissao para editar esta matricula.");
            }

            var plano = _planoRepo.Query().FirstOrDefault(p => p.Id == dto.PlanoId && p.AcademiaId == matricula.AcademiaId);
            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado.");
            }

            var forma = _formaPgtoRepo.Query().FirstOrDefault(f => f.Id == dto.FormaPagamentoId && f.AcademiaId == matricula.AcademiaId);
            if (forma == null)
            {
                throw new InvalidOperationException("Forma de pagamento nao encontrada.");
            }

            matricula.AlunoId = dto.AlunoId;
            matricula.PlanoId = dto.PlanoId;
            matricula.FormaPagamentoId = dto.FormaPagamentoId;
            matricula.DataInicio = dto.DataInicio;
            matricula.DataFim = dto.DataInicio.AddMonths(plano.DuracaoMeses);
            matricula.Status = string.IsNullOrWhiteSpace(dto.Status) ? matricula.Status : dto.Status.Trim();

            _repo.Save();
            return Get(role, new MatriculasFiltro(), matricula.AcademiaId).First(m => m.Id == matricula.Id);
        }
    }
}
