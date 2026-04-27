using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Pagamentos;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class PagamentoService
    {
        private readonly IPlanosRepository _planoRepo;
        private readonly IPagamentoRepository _pagRepo;
        private readonly IMatriculaRepository _matriculaRepo;
        private readonly IFormaPagamentoRepository _formaPagamentoRepo;

        public PagamentoService(
            IPlanosRepository planoRepo,
            IPagamentoRepository pagRepo,
            IMatriculaRepository matriculaRepo,
            IFormaPagamentoRepository formaPagamentoRepo)
        {
            _planoRepo = planoRepo;
            _pagRepo = pagRepo;
            _matriculaRepo = matriculaRepo;
            _formaPagamentoRepo = formaPagamentoRepo;
        }

        public List<Pagamentos> GetPagamentosAlunos(int alunoId)
        {
            return _pagRepo.Query()
                .Where(p => p.AlunoId == alunoId)
                .OrderByDescending(p => p.DataPagamento)
                .ToList();
        }

        public Task<Pagamentos> RegistraPagamento(PagamentoRegistroDto dto)
        {
            var plano = _planoRepo.GetByAcademia(dto.PlanoId)?.FirstOrDefault();
            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado.");
            }

            var dataPagamento = DateTime.UtcNow;
            var dataVencimento = dataPagamento.AddMonths(plano.DuracaoMeses);

            var pagamento = new Pagamentos
            {
                AlunoId = dto.AlunoId,
                PlanoId = dto.PlanoId,
                Valor = dto.Valor,
                DataPagamento = dataPagamento,
                DataVencimento = dataVencimento,
                Status = "Pg",
                AcademiaId = dto.AcademiaId
            };

            _pagRepo.Add(pagamento);
            return Task.FromResult(pagamento);
        }

        public Pagamentos ProcessarPagamentoCartaoPublico(PagamentoCartaoPublicoDto dto, int academiaId)
        {
            if (dto.AlunoId <= 0 || dto.MatriculaId <= 0 || dto.PlanoId <= 0)
            {
                throw new InvalidOperationException("Dados da matricula invalidos para processar o pagamento.");
            }

            if (dto.FormaPagamentoId <= 0)
            {
                throw new InvalidOperationException("Forma de pagamento invalida.");
            }

            ValidarCartao(dto.Cartao);

            var matricula = _matriculaRepo.Query()
                .FirstOrDefault(m =>
                    m.Id == dto.MatriculaId &&
                    m.AlunoId == dto.AlunoId &&
                    m.PlanoId == dto.PlanoId &&
                    m.AcademiaId == academiaId);

            if (matricula == null)
            {
                throw new InvalidOperationException("Matricula nao encontrada para este pagamento.");
            }

            var plano = _planoRepo.Query()
                .FirstOrDefault(p => p.Id == dto.PlanoId && p.AcademiaId == academiaId);

            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado para este aluno.");
            }

            var formaPagamento = _formaPagamentoRepo.Query()
                .FirstOrDefault(f =>
                    f.Id == dto.FormaPagamentoId &&
                    f.AcademiaId == academiaId &&
                    f.Ativo);

            if (formaPagamento == null)
            {
                throw new InvalidOperationException("Forma de pagamento nao encontrada ou indisponivel.");
            }

            if (!formaPagamento.Nome.Contains("credito", StringComparison.OrdinalIgnoreCase) &&
                !formaPagamento.Nome.Contains("crédito", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A forma de pagamento selecionada nao e valida para cartao de credito.");
            }

            var valorEsperado = plano.Valor;
            if (dto.Valor <= 0 || decimal.Round(dto.Valor, 2) != decimal.Round(valorEsperado, 2))
            {
                throw new InvalidOperationException("O valor informado nao corresponde ao plano contratado.");
            }

            var agora = DateTime.UtcNow;
            var pagamento = new Pagamentos
            {
                AcademiaId = academiaId,
                AlunoId = dto.AlunoId,
                MatriculaId = dto.MatriculaId,
                PlanoId = dto.PlanoId,
                FormaPagamentoId = dto.FormaPagamentoId,
                Valor = dto.Valor,
                DataPagamento = agora,
                DataVencimento = agora.AddMonths(plano.DuracaoMeses),
                Status = "Pago",
                ExternalId = $"CARD-{Guid.NewGuid():N}"
            };

            _pagRepo.Add(pagamento);

            matricula.MensalidadePaga = true;
            matricula.DataPagamento = agora;
            matricula.FormaPagamentoId = dto.FormaPagamentoId;

            _pagRepo.Save();

            return pagamento;
        }

        public void ProcessarWebhook(dynamic payload)
        {
            string externalId = payload?.data?.id?.ToString();
            string status = payload?.data?.status?.ToString();

            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new InvalidOperationException("ExternalId invalido.");
            }

            var pagamento = _pagRepo.Query()
                .FirstOrDefault(p => p.ExternalId == externalId);

            if (pagamento == null)
            {
                throw new InvalidOperationException("Pagamento nao encontrado.");
            }

            if (status == "approved")
            {
                pagamento.Status = "Pago";

                var matricula = _matriculaRepo.Query()
                    .FirstOrDefault(m => m.Id == pagamento.MatriculaId);

                if (matricula != null)
                {
                    matricula.MensalidadePaga = true;
                    matricula.DataPagamento = DateTime.UtcNow;
                }

                _pagRepo.Save();
            }
        }

        private static void ValidarCartao(CartaoDto cartao)
        {
            if (cartao == null)
            {
                throw new InvalidOperationException("Dados do cartao nao informados.");
            }

            var numero = SomenteDigitos(cartao.Numero);
            if (numero.Length < 13 || numero.Length > 19 || !PassaLuhn(numero))
            {
                throw new InvalidOperationException("Numero do cartao invalido.");
            }

            if (string.IsNullOrWhiteSpace(cartao.Nome) || cartao.Nome.Trim().Length < 5)
            {
                throw new InvalidOperationException("Nome impresso no cartao invalido.");
            }

            if (!ValidadeCartaoEhValida(cartao.Validade))
            {
                throw new InvalidOperationException("Validade do cartao invalida.");
            }

            var cvv = SomenteDigitos(cartao.Cvv);
            if (cvv.Length < 3 || cvv.Length > 4)
            {
                throw new InvalidOperationException("CVV invalido.");
            }
        }

        private static string SomenteDigitos(string valor)
        {
            return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
        }

        private static bool PassaLuhn(string numero)
        {
            var soma = 0;
            var dobrar = false;

            for (var i = numero.Length - 1; i >= 0; i--)
            {
                var digito = numero[i] - '0';

                if (dobrar)
                {
                    digito *= 2;
                    if (digito > 9)
                    {
                        digito -= 9;
                    }
                }

                soma += digito;
                dobrar = !dobrar;
            }

            return soma % 10 == 0;
        }

        private static bool ValidadeCartaoEhValida(string validade)
        {
            if (string.IsNullOrWhiteSpace(validade))
            {
                return false;
            }

            var partes = validade.Split('/');
            if (partes.Length != 2)
            {
                return false;
            }

            if (!int.TryParse(partes[0], out var mes) || !int.TryParse(partes[1], out var anoCurto))
            {
                return false;
            }

            if (mes < 1 || mes > 12)
            {
                return false;
            }

            var ano = 2000 + anoCurto;
            var ultimoDia = DateTime.DaysInMonth(ano, mes);
            var expiracao = new DateTime(ano, mes, ultimoDia, 23, 59, 59, DateTimeKind.Utc);

            return expiracao >= DateTime.UtcNow;
        }
    }
}
