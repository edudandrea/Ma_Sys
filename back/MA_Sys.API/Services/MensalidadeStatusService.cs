using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Mensalidades;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class MensalidadeStatusService
    {
        private readonly IMatriculaRepository _matriculaRepository;
        private readonly IPagamentoRepository _pagamentoRepository;

        public MensalidadeStatusService(IMatriculaRepository matriculaRepository, IPagamentoRepository pagamentoRepository)
        {
            _matriculaRepository = matriculaRepository;
            _pagamentoRepository = pagamentoRepository;
        }

        public MensalidadeStatusDto CalcularPorAluno(int alunoId)
        {
            var matricula = _matriculaRepository.Query()
                .Where(m => m.AlunoId == alunoId)
                .OrderByDescending(m => m.DataInicio)
                .FirstOrDefault();

            if (matricula == null)
            {
                return new MensalidadeStatusDto();
            }

            var pagamentos = _pagamentoRepository.Query()
                .Where(p => p.MatriculaId == matricula.Id)
                .OrderByDescending(p => p.DataVencimento)
                .ThenByDescending(p => p.DataPagamento)
                .ToList();

            return Calcular(matricula, pagamentos);
        }

        public MensalidadeStatusDto Calcular(Matricula matricula, IEnumerable<Pagamentos> pagamentos)
        {
            var hoje = DateTime.UtcNow.Date;
            var pagamentoList = pagamentos
                .OrderByDescending(p => p.DataVencimento)
                .ThenByDescending(p => p.DataPagamento)
                .ToList();

            var ultimoPagamento = pagamentoList.FirstOrDefault();
            var baseVencimentoAtual = ObterVencimentoBase(matricula, hoje);
            var vencimentoReferencia = baseVencimentoAtual;

            if (ultimoPagamento?.Status == "Pago" && ultimoPagamento.DataVencimento.Date > vencimentoReferencia)
            {
                vencimentoReferencia = ultimoPagamento.DataVencimento.Date;
            }

            var dias = (vencimentoReferencia - hoje).Days;
            var pagoAtual = ultimoPagamento?.Status == "Pago" && ultimoPagamento.DataVencimento.Date >= baseVencimentoAtual;

            var status = pagoAtual
                ? "Pago"
                : dias < 0
                    ? "Em atraso"
                    : dias <= 5
                        ? "Pendente"
                        : "Em dia";

            return new MensalidadeStatusDto
            {
                Status = status,
                DataVencimento = vencimentoReferencia,
                DiasParaVencimento = dias,
                UltimoPagamentoStatus = ultimoPagamento?.Status
            };
        }

        public void AtualizarMatriculaComoPaga(int matriculaId, int formaPagamentoId)
        {
            var matricula = _matriculaRepository.Query().FirstOrDefault(m => m.Id == matriculaId);
            if (matricula == null)
            {
                return;
            }

            matricula.MensalidadePaga = true;
            matricula.DataPagamento = DateTime.UtcNow;
            matricula.FormaPagamentoId = formaPagamentoId;
            _matriculaRepository.Save();
        }

        private static DateTime ObterVencimentoBase(Matricula matricula, DateTime hoje)
        {
            var diaBase = matricula.DataInicio.Day;
            var diaMes = Math.Min(diaBase, DateTime.DaysInMonth(hoje.Year, hoje.Month));
            return new DateTime(hoje.Year, hoje.Month, diaMes);
        }
    }
}
