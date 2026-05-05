using MA_Sys.API.Dto.Relatorios;
using MA_Sys.API.Security;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Services
{
    public class RelatorioService
    {
        private readonly AppDbContext _context;

        public RelatorioService(AppDbContext context)
        {
            _context = context;
        }

        public RelatorioResumoDto ObterResumo(string role, int? academiaId, int? userId, DateTime? inicio, DateTime? fim)
        {
            var dataInicio = (inicio ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).Date;
            var dataFim = (fim ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
            var hoje = DateTime.UtcNow.Date;

            if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar relatorio.");

                var filiados = _context.Filiado.AsQueryable();
                filiados = filiados.Where(f => f.OwnerUserId == userId.Value);

                var filiadoIds = filiados.Select(f => f.Id);
                var pagamentos = _context.PagamentosFiliados.Where(p => filiadoIds.Contains(p.FiliadoId));

                return MontarRelatorioEntidades(
                    "Filiados",
                    dataInicio,
                    dataFim,
                    filiados.Count(f => f.DataCadastro >= dataInicio && f.DataCadastro <= dataFim),
                    filiados.Count(f => f.Ativo),
                    filiados.Count(f => !f.Ativo),
                    pagamentos,
                    hoje);
            }

            if (RoleScope.IsAdmin(role) || RoleScope.IsSuperAdmin(role))
            {
                var academiaQuery = _context.Academias.AsQueryable();
                if (RoleScope.IsAdmin(role))
                {
                    if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
                    academiaQuery = academiaQuery.Where(a => a.OwnerUserId == userId.Value);
                }

                var academiaIds = academiaQuery.Select(a => a.Id);
                var mensalidades = _context.PagamentosAcademias
                    .Where(p => academiaIds.Contains(p.AcademiaId) &&
                        p.DataVencimento >= dataInicio &&
                        p.DataVencimento <= dataFim);

                return new RelatorioResumoDto
                {
                    Escopo = "Sistema",
                    DataInicio = dataInicio,
                    DataFim = dataFim,
                    TotalCadastrados = academiaQuery.Count(a => a.DataCadastro >= dataInicio && a.DataCadastro <= dataFim),
                    TotalAtivos = academiaQuery.Count(a => a.Ativo),
                    TotalInativos = academiaQuery.Count(a => !a.Ativo),
                    TotalMensalidadesSistema = mensalidades.Count(),
                    TotalMensalidadesSistemaPagas = mensalidades.Count(p => p.Status == "Pago"),
                    TotalMensalidadesSistemaPendentes = mensalidades.Count(p => p.Status == "Pendente" && p.DataVencimento.Date >= hoje),
                    TotalMensalidadesSistemaAtrasadas = mensalidades.Count(p => p.Status == "Pendente" && p.DataVencimento.Date < hoje),
                    ValorMensalidadesSistema = mensalidades.Sum(p => (decimal?)p.Valor) ?? 0m,
                    ValorMensalidadesSistemaRecebido = mensalidades.Where(p => p.Status == "Pago").Sum(p => (decimal?)p.Valor) ?? 0m,
                    ValorMensalidadesSistemaPendente = mensalidades.Where(p => p.Status != "Pago").Sum(p => (decimal?)p.Valor) ?? 0m
                };
            }

            if (!academiaId.HasValue) throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

            var alunos = _context.Alunos.Where(a => a.AcademiaId == academiaId.Value);
            var pagamentosAlunos = _context.Pagamentos.Where(p => p.AcademiaId == academiaId.Value);

            return MontarRelatorioAlunos(
                dataInicio,
                dataFim,
                alunos,
                pagamentosAlunos,
                hoje);
        }

        private static RelatorioResumoDto MontarRelatorioAlunos(
            DateTime dataInicio,
            DateTime dataFim,
            IQueryable<MA_SYS.Api.Models.Aluno> alunos,
            IQueryable<MA_SYS.Api.Models.Pagamentos> pagamentos,
            DateTime hoje)
        {
            var ultimosPagamentos = pagamentos
                .Where(p => p.DataVencimento >= dataInicio && p.DataVencimento <= dataFim)
                .AsEnumerable()
                .GroupBy(p => p.AlunoId)
                .Select(g => g.OrderByDescending(p => p.DataVencimento).First())
                .ToList();

            return new RelatorioResumoDto
            {
                Escopo = "Alunos",
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalCadastrados = alunos.Count(a => a.DataCadastro >= dataInicio && a.DataCadastro <= dataFim),
                TotalAtivos = alunos.Count(a => a.Ativo),
                TotalInativos = alunos.Count(a => !a.Ativo),
                TotalEmDia = ultimosPagamentos.Count(p => p.Status == "Pago" && p.DataVencimento.Date >= hoje),
                TotalPendentes = ultimosPagamentos.Count(p => p.Status == "Pendente" && p.DataVencimento.Date >= hoje),
                TotalEmAtraso = ultimosPagamentos.Count(p => p.Status == "Pendente" && p.DataVencimento.Date < hoje)
            };
        }

        private static RelatorioResumoDto MontarRelatorioEntidades(
            string escopo,
            DateTime dataInicio,
            DateTime dataFim,
            int cadastrados,
            int ativos,
            int inativos,
            IQueryable<Models.PagamentoFiliado> pagamentos,
            DateTime hoje)
        {
            var periodo = pagamentos.Where(p => p.DataVencimento >= dataInicio && p.DataVencimento <= dataFim);

            return new RelatorioResumoDto
            {
                Escopo = escopo,
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalCadastrados = cadastrados,
                TotalAtivos = ativos,
                TotalInativos = inativos,
                TotalEmDia = periodo.Count(p => p.Status == "Pago" && p.DataVencimento.Date >= hoje),
                TotalPendentes = periodo.Count(p => p.Status == "Pendente" && p.DataVencimento.Date >= hoje),
                TotalEmAtraso = periodo.Count(p => p.Status == "Pendente" && p.DataVencimento.Date < hoje)
            };
        }
    }
}
