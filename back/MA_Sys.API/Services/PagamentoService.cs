using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Pagamentos;
using MA_SYS.Api.Models;
using System.Globalization;
using System.Text;

namespace MA_Sys.API.Services
{
    public class PagamentoService
    {
        private readonly IPlanosRepository _planoRepo;
        private readonly IPagamentoRepository _pagRepo;
        private readonly IMatriculaRepository _matriculaRepo;
        private readonly IFormaPagamentoRepository _formaPagamentoRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly MercadoPagoGatewayService _mercadoPagoGateway;
        private readonly MensalidadeStatusService _mensalidadeStatusService;

        public PagamentoService(
            IPlanosRepository planoRepo,
            IPagamentoRepository pagRepo,
            IMatriculaRepository matriculaRepo,
            IFormaPagamentoRepository formaPagamentoRepo,
            IAcademiaRepository academiaRepo,
            MercadoPagoGatewayService mercadoPagoGateway,
            MensalidadeStatusService mensalidadeStatusService)
        {
            _planoRepo = planoRepo;
            _pagRepo = pagRepo;
            _matriculaRepo = matriculaRepo;
            _formaPagamentoRepo = formaPagamentoRepo;
            _academiaRepo = academiaRepo;
            _mercadoPagoGateway = mercadoPagoGateway;
            _mensalidadeStatusService = mensalidadeStatusService;
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
                Status = "Pago",
                AcademiaId = dto.AcademiaId
            };

            _pagRepo.Add(pagamento);
            _pagRepo.Save();
            return Task.FromResult(pagamento);
        }

        public Pagamentos RegistrarPagamentoManual(PagamentoManualCreateDto dto, int academiaId)
        {
            var matricula = _matriculaRepo.Query()
                .FirstOrDefault(m =>
                    m.Id == dto.MatriculaId &&
                    m.AlunoId == dto.AlunoId &&
                    m.PlanoId == dto.PlanoId &&
                    m.AcademiaId == academiaId);

            if (matricula == null)
            {
                throw new InvalidOperationException("Matricula nao encontrada para baixa manual.");
            }

            var plano = _planoRepo.Query().FirstOrDefault(p => p.Id == dto.PlanoId && p.AcademiaId == academiaId);
            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado.");
            }

            var pagamento = new Pagamentos
            {
                AcademiaId = academiaId,
                AlunoId = dto.AlunoId,
                MatriculaId = dto.MatriculaId,
                PlanoId = dto.PlanoId,
                FormaPagamentoId = dto.FormaPagamentoId,
                Valor = dto.Valor,
                DataPagamento = dto.DataPagamento ?? DateTime.UtcNow,
                DataVencimento = (dto.DataPagamento ?? DateTime.UtcNow).AddMonths(plano.DuracaoMeses),
                Status = "Pago"
            };

            _pagRepo.Add(pagamento);
            _pagRepo.Save();
            _mensalidadeStatusService.AtualizarMatriculaComoPaga(dto.MatriculaId, dto.FormaPagamentoId);

            return pagamento;
        }

        public async Task<PagamentoPixResponseDto> GerarPagamentoPixAsync(PagamentoPixCreateDto dto, int academiaId)
        {
            return await GerarPagamentoPixInternoAsync(
                academiaId,
                dto.AlunoId,
                dto.MatriculaId,
                dto.PlanoId,
                dto.FormaPagamentoId,
                dto.Valor,
                dto.Nome,
                dto.Email,
                dto.Cidade);
        }

        public async Task<PagamentoPixResponseDto> GerarPagamentoPixPublicoAsync(PagamentoPixPublicoDto dto, int academiaId)
        {
            return await GerarPagamentoPixInternoAsync(
                academiaId,
                dto.AlunoId,
                dto.MatriculaId,
                dto.PlanoId,
                dto.FormaPagamentoId,
                dto.Valor,
                dto.Nome,
                dto.Email,
                dto.Cidade);
        }

        private async Task<PagamentoPixResponseDto> GerarPagamentoPixInternoAsync(
            int academiaId,
            int alunoId,
            int matriculaId,
            int planoId,
            int formaPagamentoId,
            decimal valor,
            string? nome,
            string? email,
            string? cidade)
        {
            var matricula = _matriculaRepo.Query()
                .FirstOrDefault(m =>
                    m.Id == matriculaId &&
                    m.AlunoId == alunoId &&
                    m.PlanoId == planoId &&
                    m.AcademiaId == academiaId);

            if (matricula == null)
            {
                throw new InvalidOperationException("Matricula nao encontrada para gerar PIX.");
            }

            var plano = _planoRepo.Query().FirstOrDefault(p => p.Id == planoId && p.AcademiaId == academiaId);
            if (plano == null)
            {
                throw new InvalidOperationException("Plano nao encontrado.");
            }

            var forma = _formaPagamentoRepo.Query()
                .FirstOrDefault(f => f.Id == formaPagamentoId && f.AcademiaId == academiaId);

            if (forma == null)
            {
                throw new InvalidOperationException("Forma de pagamento nao encontrada.");
            }

            var academia = _academiaRepo.Query().FirstOrDefault(a => a.Id == academiaId);
            if (academia == null)
            {
                throw new InvalidOperationException("Academia nao encontrada.");
            }

            var pagamento = new Pagamentos
            {
                AcademiaId = academiaId,
                AlunoId = alunoId,
                MatriculaId = matriculaId,
                PlanoId = planoId,
                FormaPagamentoId = formaPagamentoId,
                Valor = valor,
                DataPagamento = DateTime.UtcNow,
                DataVencimento = matricula.DataInicio.AddMonths(plano.DuracaoMeses),
                Status = "Pendente"
            };

            if (string.IsNullOrWhiteSpace(academia.MercadoPagoAccessToken))
            {
                throw new InvalidOperationException("A academia ainda nao configurou o token do Mercado Pago para PIX.");
            }

            var descricao = $"Mensalidade {plano.Nome} - Aluno {alunoId}";
            var gatewayResult = await _mercadoPagoGateway.CriarPagamentoPixAsync(
                valor,
                descricao,
                string.IsNullOrWhiteSpace(email) ? "pagador@aluno.local" : email!,
                academia.MercadoPagoAccessToken);

            if (string.IsNullOrWhiteSpace(gatewayResult.Payload) &&
                string.IsNullOrWhiteSpace(gatewayResult.QrCodeBase64))
            {
                throw new InvalidOperationException("O Mercado Pago gerou a cobranca, mas nao retornou QR Code nem codigo copia e cola do PIX.");
            }

            pagamento.ExternalId = gatewayResult.ExternalId;
            pagamento.Status = MapearStatusGateway(gatewayResult.Status);
            _pagRepo.Add(pagamento);
            _pagRepo.Save();

            if (pagamento.Status == "Pago")
            {
                _mensalidadeStatusService.AtualizarMatriculaComoPaga(matriculaId, formaPagamentoId);
            }

            return new PagamentoPixResponseDto
            {
                PagamentoId = pagamento.Id,
                Status = pagamento.Status,
                Payload = gatewayResult.Payload,
                QrCodeBase64 = gatewayResult.QrCodeBase64,
                ExternalId = gatewayResult.ExternalId,
                VerificacaoAutomaticaDisponivel = true,
                Mensagem = "Cobranca PIX gerada com verificacao automatica habilitada."
            };
        }

        public async Task<Pagamentos> ProcessarPagamentoCartaoPublico(PagamentoCartaoPublicoDto dto, int academiaId)
        {
            if (dto.AlunoId <= 0 || dto.MatriculaId <= 0 || dto.PlanoId <= 0)
            {
                throw new InvalidOperationException("Dados da matricula invalidos para processar o pagamento.");
            }

            if (dto.FormaPagamentoId <= 0)
            {
                throw new InvalidOperationException("Forma de pagamento invalida.");
            }

            if (string.IsNullOrWhiteSpace(dto.CardToken))
            {
                throw new InvalidOperationException("Token de cartao nao informado.");
            }

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

            var formaPagamentoNormalizada = NormalizarTexto(formaPagamento.Nome);
            if (!formaPagamentoNormalizada.Contains("credito") &&
                !formaPagamentoNormalizada.Contains("cartao"))
            {
                throw new InvalidOperationException("A forma de pagamento selecionada nao e valida para cartao de credito.");
            }

            var valorEsperado = plano.Valor;
            if (dto.Valor <= 0 || decimal.Round(dto.Valor, 2) != decimal.Round(valorEsperado, 2))
            {
                throw new InvalidOperationException("O valor informado nao corresponde ao plano contratado.");
            }

            var academia = _academiaRepo.Query()
                .FirstOrDefault(a => a.Id == academiaId);

            if (academia == null)
            {
                throw new InvalidOperationException("Academia nao encontrada para processar o pagamento.");
            }

            if (string.IsNullOrWhiteSpace(academia.MercadoPagoAccessToken))
            {
                throw new InvalidOperationException("A academia ainda nao configurou o token de recebimento do cartao.");
            }

            var descricao = $"Plano {plano.Nome} - Aluno {dto.AlunoId}";
            var gatewayResult = await _mercadoPagoGateway.ProcessarPagamentoCartaoAsync(
                dto,
                descricao,
                academia.MercadoPagoAccessToken);

            var statusPagamento = gatewayResult.Status.ToLowerInvariant() switch
            {
                "approved" => "Pago",
                "pending" => "Pendente",
                "in_process" => "EmAnalise",
                _ => "Recusado"
            };

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
                Status = statusPagamento,
                ExternalId = gatewayResult.ExternalId
            };

            _pagRepo.Add(pagamento);

            if (statusPagamento == "Pago")
            {
                _mensalidadeStatusService.AtualizarMatriculaComoPaga(dto.MatriculaId, dto.FormaPagamentoId);
            }

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
                _mensalidadeStatusService.AtualizarMatriculaComoPaga(pagamento.MatriculaId, pagamento.FormaPagamentoId);

                _pagRepo.Save();
            }
        }

        public Pagamentos? ObterPagamentoPorId(int pagamentoId, int academiaId)
        {
            return _pagRepo.Query()
                .FirstOrDefault(p => p.Id == pagamentoId && p.AcademiaId == academiaId);
        }

        public Pagamentos AtualizarStatusPagamento(int pagamentoId, int academiaId)
        {
            var pagamento = _pagRepo.Query()
                .FirstOrDefault(p => p.Id == pagamentoId && p.AcademiaId == academiaId);

            if (pagamento == null)
            {
                throw new InvalidOperationException("Pagamento nao encontrado.");
            }

            if (string.IsNullOrWhiteSpace(pagamento.ExternalId))
            {
                return pagamento;
            }

            var accessToken = _academiaRepo.Query()
                .Where(a => a.Id == academiaId)
                .Select(a => a.MercadoPagoAccessToken)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return pagamento;
            }

            var gatewayResult = _mercadoPagoGateway.ConsultarPagamentoAsync(pagamento.ExternalId, accessToken)
                .GetAwaiter()
                .GetResult();

            pagamento.Status = MapearStatusGateway(gatewayResult.Status);

            if (pagamento.Status == "Pago")
            {
                _mensalidadeStatusService.AtualizarMatriculaComoPaga(pagamento.MatriculaId, pagamento.FormaPagamentoId);
            }

            _pagRepo.Save();
            return pagamento;
        }

        private static string MapearStatusGateway(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "approved" => "Pago",
                "pending" => "Pendente",
                "in_process" => "EmAnalise",
                _ => "Recusado"
            };
        }

        private static string NormalizarTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            var decomposed = valor.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
        }

    }
}
