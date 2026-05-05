using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.FluxoCaixa;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Services
{
    public class FluxoCaixaService
    {
        private const string FormaPagamentoMarker = " [Forma pagamento: ";
        private readonly IFinanceiroRepository _financeiroRepo;
        private readonly IPagamentoRepository _pagamentoRepo;
        private readonly IPagamentoAcademiaRepository _pagamentoAcademiaRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly AppDbContext _context;

        public FluxoCaixaService(
            IFinanceiroRepository financeiroRepo,
            IPagamentoRepository pagamentoRepo,
            IPagamentoAcademiaRepository pagamentoAcademiaRepo,
            IAcademiaRepository academiaRepo,
            AppDbContext context)
        {
            _financeiroRepo = financeiroRepo;
            _pagamentoRepo = pagamentoRepo;
            _pagamentoAcademiaRepo = pagamentoAcademiaRepo;
            _academiaRepo = academiaRepo;
            _context = context;
        }

        public FluxoCaixaResponseDto Listar(string role, int? academiaId, int? userId, FluxoCaixaFiltroDto filtro)
        {
            if (RoleScope.IsFederacao(role))
            {
                return ListarFederacao(userId, filtro);
            }

            var movimentos = new List<FluxoCaixaMovimentoDto>();
            var academias = FiltrarAcademias(role, academiaId, userId, filtro.AcademiaId)
                .Select(a => new { a.Id, Nome = a.Nome ?? $"Academia {a.Id}" })
                .ToList();

            var academiaIds = academias.Select(a => a.Id).ToList();
            if (academiaIds.Count == 0)
            {
                return new FluxoCaixaResponseDto();
            }

            var dataInicio = filtro.DataInicio?.Date ?? DateTime.UtcNow.Date.AddMonths(-1);
            var dataFim = filtro.DataFim?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            movimentos.AddRange(_financeiroRepo.Query()
                .Where(f => f.AcademiaId.HasValue && academiaIds.Contains(f.AcademiaId.Value) && f.Data >= dataInicio && f.Data <= dataFim)
                .ToList()
                .Join(
                    academias,
                    f => f.AcademiaId!.Value,
                    a => a.Id,
                    (f, a) => new FluxoCaixaMovimentoDto
                    {
                        Tipo = f.Tipo,
                        Origem = f.Origem,
                        Categoria = f.Categoria,
                        Descricao = f.Descricao,
                        AcademiaNome = a.Nome,
                        Valor = f.Valor,
                        Data = f.Data,
                        Status = f.Pago ? "Pago" : "Pendente"
                    })
                .ToList());

            movimentos.AddRange(_pagamentoRepo.Query()
                .Where(p => academiaIds.Contains(p.AcademiaId) &&
                            p.Status == "Pago" &&
                            p.DataPagamento >= dataInicio &&
                            p.DataPagamento <= dataFim)
                .ToList()
                .Join(
                    academias,
                    p => p.AcademiaId,
                    a => a.Id,
                    (p, a) => new FluxoCaixaMovimentoDto
                    {
                        Tipo = "Entrada",
                        Origem = "Mensalidade",
                        Categoria = "Aluno",
                        Descricao = $"Pagamento de aluno #{p.AlunoId}",
                        AcademiaNome = a.Nome,
                        Valor = p.Valor,
                        Data = p.DataPagamento,
                        Status = p.Status
                    })
                .ToList());

            var pagamentosAcademia = _pagamentoAcademiaRepo.Query()
                .Where(p => academiaIds.Contains(p.AcademiaId) &&
                            p.Status == "Pago" &&
                            p.DataPagamento.HasValue &&
                            p.DataPagamento.Value >= dataInicio &&
                            p.DataPagamento.Value <= dataFim)
                .ToList()
                .Join(
                    academias,
                    p => p.AcademiaId,
                    a => a.Id,
                    (p, a) => new { Pagamento = p, AcademiaNome = a.Nome })
                .ToList();

            movimentos.AddRange(pagamentosAcademia.Select(x => new FluxoCaixaMovimentoDto
            {
                Tipo = RoleScope.IsAcademia(role) ? "Saida" : "Entrada",
                Origem = "Sistema",
                Categoria = "Mensalidade da academia",
                Descricao = SanitizarDescricao(x.Pagamento.Descricao) ?? "Mensalidade do sistema",
                AcademiaNome = x.AcademiaNome,
                Valor = x.Pagamento.Valor,
                Data = x.Pagamento.DataPagamento ?? x.Pagamento.DataCriacao,
                Status = x.Pagamento.Status,
                FormaPagamentoNome = ExtrairFormaPagamentoNome(x.Pagamento.Descricao),
            }));

            movimentos = movimentos
                .OrderByDescending(m => m.Data)
                .ToList();

            var totalEntradas = movimentos
                .Where(m => string.Equals(m.Tipo, "Entrada", StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Valor);

            var totalSaidas = movimentos
                .Where(m => string.Equals(m.Tipo, "Saida", StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Valor);

            return new FluxoCaixaResponseDto
            {
                Resumo = new FluxoCaixaResumoDto
                {
                    TotalEntradas = totalEntradas,
                    TotalSaidas = totalSaidas,
                    Saldo = totalEntradas - totalSaidas,
                    TotalMovimentos = movimentos.Count
                },
                Movimentos = movimentos
            };
        }

        public void Lancar(string role, int? academiaId, int? userId, FluxoCaixaCreateDto dto)
        {
            if (RoleScope.IsFederacao(role))
            {
                LancarFederacao(userId, dto);
                return;
            }

            var academiaDestino = RoleScope.IsAcademia(role) ? academiaId ?? 0 : dto.AcademiaId ?? 0;
            var academiaValida = FiltrarAcademias(role, academiaId, userId, academiaDestino)
                .Any(a => a.Id == academiaDestino);

            if (!academiaValida)
                throw new UnauthorizedAccessException("Academia fora do escopo do usuario.");

            if (dto.Valor <= 0)
                throw new InvalidOperationException("Valor invalido para o lancamento.");

            if (string.IsNullOrWhiteSpace(dto.Tipo))
                throw new InvalidOperationException("Tipo do lancamento e obrigatorio.");

            var lancamento = new Financeiro
            {
                AcademiaId = academiaDestino,
                Valor = dto.Valor,
                Data = dto.Data == default ? DateTime.UtcNow : dto.Data,
                Tipo = dto.Tipo.Trim(),
                Categoria = dto.Categoria?.Trim() ?? string.Empty,
                Descricao = dto.Descricao?.Trim() ?? string.Empty,
                FormaPagamentoId = dto.FormaPagamentoId,
                Origem = "Manual",
                Pago = dto.Pago
            };

            _financeiroRepo.Add(lancamento);
            _financeiroRepo.Save();
        }

        private IQueryable<MA_SYS.Api.Models.Academia> FiltrarAcademias(string role, int? academiaId, int? userId, int? academiaIdFiltro)
        {
            var query = _academiaRepo.Query();

            if (RoleScope.IsSuperAdmin(role))
            {
                if (academiaIdFiltro.HasValue)
                {
                    query = query.Where(a => a.Id == academiaIdFiltro.Value);
                }

                return query;
            }

            if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(a => a.OwnerUserId == userId.Value);

                if (academiaIdFiltro.HasValue)
                {
                    query = query.Where(a => a.Id == academiaIdFiltro.Value);
                }

                return query;
            }

            if (!academiaId.HasValue)
                throw new UnauthorizedAccessException("Usuario sem academia valida.");

            return query.Where(a => a.Id == academiaId.Value);
        }

        private FluxoCaixaResponseDto ListarFederacao(int? userId, FluxoCaixaFiltroDto filtro)
        {
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Usuario de federacao invalido.");

            var dataInicio = filtro.DataInicio?.Date ?? DateTime.UtcNow.Date.AddMonths(-1);
            var dataFim = filtro.DataFim?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var movimentos = _context.PagamentosFiliados
                .Where(p =>
                    p.Filiado != null &&
                    p.Filiado.OwnerUserId == userId.Value &&
                    p.DataVencimento >= dataInicio &&
                    p.DataVencimento <= dataFim)
                .Select(p => new FluxoCaixaMovimentoDto
                {
                    Tipo = "Entrada",
                    Origem = "Filiado",
                    Categoria = "Cobranca de filiado",
                    Descricao = p.Descricao ?? $"Cobranca do filiado #{p.FiliadoId}",
                    AcademiaNome = p.Filiado != null ? p.Filiado.Nome ?? $"Filiado {p.FiliadoId}" : $"Filiado {p.FiliadoId}",
                    Valor = p.Valor,
                    Data = p.DataPagamento ?? p.DataVencimento,
                    Status = p.Status,
                    FormaPagamentoNome = p.FormaPagamentoNome
                })
                .OrderByDescending(m => m.Data)
                .ToList();

            movimentos.AddRange(_financeiroRepo.Query()
                .Where(f =>
                    f.OwnerUserId == userId.Value &&
                    f.Data >= dataInicio &&
                    f.Data <= dataFim)
                .Select(f => new FluxoCaixaMovimentoDto
                {
                    Tipo = f.Tipo,
                    Origem = f.Origem,
                    Categoria = f.Categoria,
                    Descricao = f.Descricao,
                    AcademiaNome = "Federacao",
                    Valor = f.Valor,
                    Data = f.Data,
                    Status = f.Pago ? "Pago" : "Pendente",
                    FormaPagamentoNome = f.FormaPagamento != null ? f.FormaPagamento.Nome : null
                })
                .ToList());

            movimentos = movimentos
                .OrderByDescending(m => m.Data)
                .ToList();

            var totalEntradas = movimentos
                .Where(m =>
                    string.Equals(m.Status, "Pago", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.Tipo, "Entrada", StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Valor);

            var totalSaidas = movimentos
                .Where(m =>
                    string.Equals(m.Status, "Pago", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.Tipo, "Saida", StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Valor);

            return new FluxoCaixaResponseDto
            {
                Resumo = new FluxoCaixaResumoDto
                {
                    TotalEntradas = totalEntradas,
                    TotalSaidas = totalSaidas,
                    Saldo = totalEntradas - totalSaidas,
                    TotalMovimentos = movimentos.Count
                },
                Movimentos = movimentos
            };
        }

        private void LancarFederacao(int? userId, FluxoCaixaCreateDto dto)
        {
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Usuario de federacao invalido.");

            if (dto.Valor <= 0)
                throw new InvalidOperationException("Valor invalido para o lancamento.");

            if (string.IsNullOrWhiteSpace(dto.Tipo))
                throw new InvalidOperationException("Tipo do lancamento e obrigatorio.");

            var lancamento = new Financeiro
            {
                AcademiaId = null,
                OwnerUserId = userId.Value,
                Valor = dto.Valor,
                Data = dto.Data == default ? DateTime.UtcNow : dto.Data,
                Tipo = dto.Tipo.Trim(),
                Categoria = dto.Categoria?.Trim() ?? string.Empty,
                Descricao = dto.Descricao?.Trim() ?? string.Empty,
                FormaPagamentoId = dto.FormaPagamentoId,
                Origem = "Manual",
                Pago = dto.Pago
            };

            _financeiroRepo.Add(lancamento);
            _financeiroRepo.Save();
        }

        private static string? SanitizarDescricao(string? descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return descricao;

            var markerIndex = descricao.IndexOf(FormaPagamentoMarker, StringComparison.Ordinal);
            if (markerIndex < 0)
                return descricao;

            return descricao[..markerIndex].TrimEnd();
        }

        private static string? ExtrairFormaPagamentoNome(string? descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var markerIndex = descricao.IndexOf(FormaPagamentoMarker, StringComparison.Ordinal);
            if (markerIndex < 0)
                return null;

            var inicio = markerIndex + FormaPagamentoMarker.Length;
            var fim = descricao.IndexOf(']', inicio);
            if (fim < 0)
                return null;

            return descricao[inicio..fim].Trim();
        }
    }
}
