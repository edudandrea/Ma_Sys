using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.PagamentosAcademias;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using MA_Sys.API.Dto.Pagamentos;

namespace MA_Sys.API.Services
{
    public class PagamentoAcademiaService
    {
        private readonly IPagamentoAcademiaRepository _pagamentoAcademiaRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly IMensalidadeSistemaRepository _mensalidadeSistemaRepo;
        private readonly MercadoPagoGatewayService _mercadoPagoGateway;

        public PagamentoAcademiaService(
            IPagamentoAcademiaRepository pagamentoAcademiaRepo,
            IAcademiaRepository academiaRepo,
            IMensalidadeSistemaRepository mensalidadeSistemaRepo,
            MercadoPagoGatewayService mercadoPagoGateway)
        {
            _pagamentoAcademiaRepo = pagamentoAcademiaRepo;
            _academiaRepo = academiaRepo;
            _mensalidadeSistemaRepo = mensalidadeSistemaRepo;
            _mercadoPagoGateway = mercadoPagoGateway;
        }

        public List<PagamentoAcademiaResponseDto> Listar(string role, int? academiaId, int? userId, int? academiaIdFiltro = null)
        {
            var query = _pagamentoAcademiaRepo.Query();

            if (RoleScope.IsSuperAdmin(role))
            {
                if (academiaIdFiltro.HasValue)
                {
                    query = query.Where(x => x.AcademiaId == academiaIdFiltro.Value);
                }
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(x => academiaIds.Contains(x.AcademiaId));

                if (academiaIdFiltro.HasValue)
                {
                    query = query.Where(x => x.AcademiaId == academiaIdFiltro.Value);
                }
            }
            else
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(x => x.AcademiaId == academiaId.Value);
            }

            return query
                .Join(_academiaRepo.Query(),
                    p => p.AcademiaId,
                    a => a.Id,
                    (p, a) => new PagamentoAcademiaResponseDto
                    {
                        Id = p.Id,
                        AcademiaId = p.AcademiaId,
                        NomeAcademia = a.Nome ?? $"Academia {a.Id}",
                        Valor = p.Valor,
                        DataCriacao = p.DataCriacao,
                        DataVencimento = p.DataVencimento,
                        DataPagamento = p.DataPagamento,
                        Status = p.Status,
                        Descricao = p.Descricao,
                        FormaPagamentoNome = p.FormaPagamentoNome,
                        MensalidadeSistemaId = p.MensalidadeSistemaId
                    })
                .OrderByDescending(x => x.DataVencimento)
                .ToList();
        }

        public PagamentoAcademia CriarCobranca(PagamentoAcademiaCreateDto dto, string role, int? userId)
        {
            var academiaExiste = _academiaRepo.Query()
                .Any(a => a.Id == dto.AcademiaId &&
                    (RoleScope.IsSuperAdmin(role) ||
                     (RoleScope.IsAdmin(role) && userId.HasValue && a.OwnerUserId == userId.Value)));
            if (!academiaExiste)
                throw new InvalidOperationException("Academia nao encontrada.");

            if (dto.Valor <= 0)
                throw new InvalidOperationException("Valor da cobranca invalido.");

            if (dto.MensalidadeSistemaId.HasValue)
            {
                ValidarMensalidadeNoEscopo(dto.MensalidadeSistemaId.Value, role, userId);
            }

            var cobranca = new PagamentoAcademia
            {
                AcademiaId = dto.AcademiaId,
                Valor = dto.Valor,
                DataCriacao = DateTime.UtcNow,
                DataVencimento = dto.DataVencimento.Date,
                Descricao = dto.Descricao?.Trim(),
                Status = "Pendente",
                MensalidadeSistemaId = dto.MensalidadeSistemaId
            };

            _pagamentoAcademiaRepo.Add(cobranca);
            _pagamentoAcademiaRepo.Save();
            return cobranca;
        }

        public async Task<PagamentoAcademiaPixResponseDto> GerarPagamentoPixAsync(int id, string role, int? academiaId, int? userId)
        {
            var cobranca = ObterCobrancaPorEscopo(id, role, academiaId, userId);

            if (cobranca.Status == "Pago")
            {
                return new PagamentoAcademiaPixResponseDto
                {
                    PagamentoId = cobranca.Id,
                    Status = cobranca.Status,
                    ExternalId = cobranca.ExternalId,
                    VerificacaoAutomaticaDisponivel = true,
                    Mensagem = "Esta cobranca ja foi paga."
                };
            }

            var gatewayResult = await _mercadoPagoGateway.CriarPagamentoPixAsync(
                cobranca.Valor,
                cobranca.Descricao ?? $"Mensalidade do sistema - Academia {cobranca.AcademiaId}",
                "financeiro@marcialprox.local");

            cobranca.ExternalId = gatewayResult.ExternalId;
            cobranca.FormaPagamentoNome = "PIX";
            cobranca.Status = MapearStatusGateway(gatewayResult.Status);

            if (cobranca.Status == "Pago")
            {
                cobranca.DataPagamento = DateTime.UtcNow;
            }

            _pagamentoAcademiaRepo.Update(cobranca);
            _pagamentoAcademiaRepo.Save();

            return new PagamentoAcademiaPixResponseDto
            {
                PagamentoId = cobranca.Id,
                Status = cobranca.Status,
                Payload = gatewayResult.Payload,
                QrCodeBase64 = gatewayResult.QrCodeBase64,
                ExternalId = gatewayResult.ExternalId,
                VerificacaoAutomaticaDisponivel = true,
                Mensagem = cobranca.Status == "Pago"
                    ? "Pagamento PIX aprovado."
                    : "Cobranca PIX gerada. Aguarde a confirmacao do pagamento."
            };
        }

        public async Task<PagamentoAcademiaStatusDto> PagarComCartaoAsync(int id, PagamentoAcademiaCartaoDto dto, string role, int? academiaId, int? userId)
        {
            var cobranca = ObterCobrancaPorEscopo(id, role, academiaId, userId);

            if (cobranca.Status == "Pago")
            {
                return MapToStatus(cobranca);
            }

            if (string.IsNullOrWhiteSpace(dto.CardToken))
                throw new InvalidOperationException("Token do cartao nao informado.");

            var gatewayDto = new PagamentoCartaoPublicoDto
            {
                Valor = cobranca.Valor,
                Parcelas = dto.Parcelas <= 0 ? 1 : dto.Parcelas,
                PayerEmail = dto.PayerEmail,
                CardToken = dto.CardToken,
                PaymentMethodId = dto.PaymentMethodId
            };

            var gatewayResult = await _mercadoPagoGateway.ProcessarPagamentoCartaoAsync(
                gatewayDto,
                cobranca.Descricao ?? $"Mensalidade do sistema - Academia {cobranca.AcademiaId}");

            cobranca.ExternalId = gatewayResult.ExternalId;
            cobranca.FormaPagamentoNome = "Cartao";
            cobranca.Status = MapearStatusGateway(gatewayResult.Status);

            if (cobranca.Status == "Pago")
            {
                cobranca.DataPagamento = DateTime.UtcNow;
            }

            _pagamentoAcademiaRepo.Update(cobranca);
            _pagamentoAcademiaRepo.Save();

            return MapToStatus(cobranca);
        }

        public void BaixarPagamento(int id, string role, int? userId)
        {
            var query = _pagamentoAcademiaRepo.Query().Where(p => p.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(p => p.Academia.OwnerUserId == userId.Value);
            }
            else
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para baixar cobranca.");
            }

            var cobranca = query.FirstOrDefault();
            if (cobranca == null)
                throw new InvalidOperationException("Cobranca nao encontrada.");

            cobranca.Status = "Pago";
            cobranca.DataPagamento = DateTime.UtcNow;

            _pagamentoAcademiaRepo.Update(cobranca);
            _pagamentoAcademiaRepo.Save();
        }

        public PagamentoAcademiaStatusDto AtualizarStatusPagamento(int id, string role, int? academiaId, int? userId)
        {
            var cobranca = ObterCobrancaPorEscopo(id, role, academiaId, userId);

            if (string.IsNullOrWhiteSpace(cobranca.ExternalId))
            {
                return MapToStatus(cobranca);
            }

            var gatewayResult = _mercadoPagoGateway.ConsultarPagamentoAsync(cobranca.ExternalId)
                .GetAwaiter()
                .GetResult();

            cobranca.Status = MapearStatusGateway(gatewayResult.Status);

            if (cobranca.Status == "Pago" && !cobranca.DataPagamento.HasValue)
            {
                cobranca.DataPagamento = DateTime.UtcNow;
            }

            _pagamentoAcademiaRepo.Update(cobranca);
            _pagamentoAcademiaRepo.Save();

            return MapToStatus(cobranca);
        }

        private PagamentoAcademiaResponseDto MapToResponse(PagamentoAcademia pagamento)
        {
            var academia = pagamento.Academia ?? _academiaRepo.Query().First(a => a.Id == pagamento.AcademiaId);

            return new PagamentoAcademiaResponseDto
            {
                Id = pagamento.Id,
                AcademiaId = pagamento.AcademiaId,
                NomeAcademia = academia.Nome ?? $"Academia {academia.Id}",
                Valor = pagamento.Valor,
                DataCriacao = pagamento.DataCriacao,
                DataVencimento = pagamento.DataVencimento,
                DataPagamento = pagamento.DataPagamento,
                Status = pagamento.Status,
                Descricao = pagamento.Descricao,
                FormaPagamentoNome = pagamento.FormaPagamentoNome,
                MensalidadeSistemaId = pagamento.MensalidadeSistemaId,
            };
        }

        private PagamentoAcademiaStatusDto MapToStatus(PagamentoAcademia pagamento)
        {
            return new PagamentoAcademiaStatusDto
            {
                PagamentoId = pagamento.Id,
                Status = pagamento.Status,
                FormaPagamentoNome = pagamento.FormaPagamentoNome,
                DataPagamento = pagamento.DataPagamento
            };
        }

        private PagamentoAcademia ObterCobrancaPorEscopo(int id, string role, int? academiaId, int? userId)
        {
            var query = _pagamentoAcademiaRepo.Query().Where(p => p.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(p => p.Academia.OwnerUserId == userId.Value);
            }
            else
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(p => p.AcademiaId == academiaId.Value);
            }

            var cobranca = query.FirstOrDefault();
            if (cobranca == null)
                throw new InvalidOperationException("Cobranca nao encontrada.");

            return cobranca;
        }

        private void ValidarMensalidadeNoEscopo(int mensalidadeSistemaId, string role, int? userId)
        {
            var query = _mensalidadeSistemaRepo.Query().Where(x => x.Id == mensalidadeSistemaId);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(x => x.OwnerUserId == userId.Value);
            }
            else
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para selecionar mensalidades do sistema.");
            }

            if (!query.Any())
                throw new InvalidOperationException("Mensalidade do sistema nao encontrada para o escopo informado.");
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
    }
}
