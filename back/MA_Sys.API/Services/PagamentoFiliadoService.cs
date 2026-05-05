using MA_Sys.API.Dto.PagamentosAcademias;
using MA_Sys.API.Dto.Pagamentos;
using MA_Sys.API.Dto.PagamentosFiliados;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Services
{
    public class PagamentoFiliadoService
    {
        private readonly AppDbContext _context;
        private readonly MercadoPagoGatewayService _mercadoPagoGateway;

        public PagamentoFiliadoService(AppDbContext context, MercadoPagoGatewayService mercadoPagoGateway)
        {
            _context = context;
            _mercadoPagoGateway = mercadoPagoGateway;
        }

        public List<PagamentoFiliadoResponseDto> Listar(string role, int? filiadoIdUsuario, int? userId, int? filiadoIdFiltro)
        {
            var query = _context.PagamentosFiliados.AsQueryable();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario invalido para acessar cobrancas.");
                var ids = _context.Filiado.Where(f => f.OwnerUserId == userId.Value).Select(f => f.Id);
                query = query.Where(p => ids.Contains(p.FiliadoId));
            }
            else
            {
                if (!filiadoIdUsuario.HasValue) throw new UnauthorizedAccessException("Usuario sem vinculo com federacao.");
                query = query.Where(p => p.FiliadoId == filiadoIdUsuario.Value);
            }

            if (filiadoIdFiltro.HasValue)
            {
                query = query.Where(p => p.FiliadoId == filiadoIdFiltro.Value);
            }

            return query
                .Join(_context.Filiado,
                    p => p.FiliadoId,
                    f => f.Id,
                    (p, f) => new PagamentoFiliadoResponseDto
                    {
                        Id = p.Id,
                        FiliadoId = p.FiliadoId,
                        NomeFiliado = f.Nome,
                        Valor = p.Valor,
                        DataCriacao = p.DataCriacao,
                        DataVencimento = p.DataVencimento,
                        DataPagamento = p.DataPagamento,
                        Status = p.Status,
                        Descricao = p.Descricao,
                        FormaPagamentoNome = p.FormaPagamentoNome
                    })
                .OrderByDescending(p => p.DataVencimento)
                .ToList();
        }

        public PagamentoFiliado CriarCobranca(PagamentoFiliadoCreateDto dto, string role, int? userId)
        {
            if (dto.Valor <= 0) throw new InvalidOperationException("Valor da cobranca invalido.");

            var filiadoExiste = _context.Filiado.Any(f =>
                f.Id == dto.FiliadoId &&
                (RoleScope.IsSuperAdmin(role) ||
                 (RoleScope.IsFederacao(role) && userId.HasValue && f.OwnerUserId == userId.Value) ||
                 (RoleScope.IsAdmin(role) && userId.HasValue && f.OwnerUserId == userId.Value)));

            if (!filiadoExiste) throw new InvalidOperationException("Filiado nao encontrado.");

            var cobranca = new PagamentoFiliado
            {
                FiliadoId = dto.FiliadoId,
                Valor = dto.Valor,
                DataCriacao = DateTime.UtcNow,
                DataVencimento = dto.DataVencimento.Date,
                Descricao = dto.Descricao?.Trim(),
                Status = "Pendente"
            };

            _context.PagamentosFiliados.Add(cobranca);
            _context.SaveChanges();
            return cobranca;
        }

        public async Task<PagamentoAcademiaPixResponseDto> GerarPixAsync(int id, string role, int? filiadoIdUsuario, int? userId)
        {
            var cobranca = ObterCobrancaPorEscopo(id, role, filiadoIdUsuario, userId);
            var filiado = _context.Filiado.First(f => f.Id == cobranca.FiliadoId);

            var token = ObterAccessTokenFederacaoPorUsuarioId(filiado.OwnerUserId);
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Configure o Access Token do Mercado Pago na federacao para receber cobrancas de filiados.");
            }

            var result = await _mercadoPagoGateway.CriarPagamentoPixAsync(
                cobranca.Valor,
                cobranca.Descricao ?? $"Mensalidade - {filiado.Nome}",
                !string.IsNullOrWhiteSpace(filiado.Email) ? filiado.Email! : "pagador@marcialprox.com.br",
                token);

            cobranca.ExternalId = result.ExternalId;
            cobranca.FormaPagamentoNome = "PIX";
            cobranca.Status = MapearStatusGateway(result.Status);
            if (cobranca.Status == "Pago")
            {
                cobranca.DataPagamento = DateTime.UtcNow;
            }

            _context.SaveChanges();

            return new PagamentoAcademiaPixResponseDto
            {
                PagamentoId = cobranca.Id,
                Status = cobranca.Status,
                Payload = result.Payload,
                QrCodeBase64 = result.QrCodeBase64,
                ExternalId = result.ExternalId,
                VerificacaoAutomaticaDisponivel = true,
                Mensagem = "Cobranca PIX gerada para o filiado."
            };
        }

        public object ObterConfiguracaoPublica(int federacaoId)
        {
            var federacao = ObterFederacaoPublica(federacaoId);

            return new
            {
                federacaoId = federacao.Id,
                nome = federacao.Nome,
                logoUrl = federacao.LogoUrl,
                mercadoPagoPublicKey = federacao.MercadoPagoPublicKey
            };
        }

        public List<object> ListarFormasPagamentoPublicas(int federacaoId)
        {
            var federacao = ObterFederacaoPublica(federacaoId);
            var ownerUserId = federacao.OwnerUserId
                ?? throw new InvalidOperationException("Federacao sem usuario responsavel configurado.");
            var possuiAccessToken = !string.IsNullOrWhiteSpace(federacao.MercadoPagoAccessToken);

            return _context.FormaPagamentos
                .Where(f => f.OwnerUserId == ownerUserId && f.Ativo)
                .AsEnumerable()
                .Select(f =>
                {
                    var nomeNormalizado = NormalizarTexto(f.Nome);
                    var isPix = nomeNormalizado.Contains("pix");
                    var isCartao = nomeNormalizado.Contains("cartao") || nomeNormalizado.Contains("credito");

                    if ((isPix || isCartao) &&
                        possuiAccessToken &&
                        (!isCartao || !string.IsNullOrWhiteSpace(federacao.MercadoPagoPublicKey)))
                    {
                        return new
                        {
                            id = isPix ? "pix" : "cartao",
                            nome = f.Nome,
                            parcelas = Math.Max(f.Parcelas, 1),
                            ativo = f.Ativo
                        };
                    }

                    return null;
                })
                .Where(f => f != null)
                .Cast<object>()
                .ToList();
        }

        public object BuscarFiliadoPublico(int federacaoId, PagamentoFiliadoBuscaPublicaDto busca)
        {
            var federacao = ObterFederacaoPublica(federacaoId);
            var ownerUserId = federacao.OwnerUserId
                ?? throw new InvalidOperationException("Federacao sem usuario responsavel configurado.");

            var email = busca.Email?.Trim().ToLowerInvariant();
            var telefone = ApenasDigitos(busca.Telefone);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(telefone))
            {
                throw new InvalidOperationException("Informe email e telefone para localizar o filiado.");
            }

            var filiado = _context.Filiado.FirstOrDefault(f =>
                f.OwnerUserId == ownerUserId &&
                f.Ativo &&
                f.Email != null &&
                f.Email.ToLower() == email &&
                ApenasDigitos(f.Telefone) == telefone);

            if (filiado == null)
            {
                throw new KeyNotFoundException("Filiado nao encontrado.");
            }

            var cobrancas = _context.PagamentosFiliados
                .Where(p => p.FiliadoId == filiado.Id && p.Status != "Pago")
                .OrderBy(p => p.DataVencimento)
                .Select(p => new PagamentoFiliadoResponseDto
                {
                    Id = p.Id,
                    FiliadoId = p.FiliadoId,
                    NomeFiliado = filiado.Nome,
                    Valor = p.Valor,
                    DataCriacao = p.DataCriacao,
                    DataVencimento = p.DataVencimento,
                    DataPagamento = p.DataPagamento,
                    Status = p.Status,
                    Descricao = p.Descricao,
                    FormaPagamentoNome = p.FormaPagamentoNome
                })
                .ToList();

            return new
            {
                filiadoId = filiado.Id,
                nome = filiado.Nome,
                email = filiado.Email,
                telefone = filiado.Telefone,
                cidade = filiado.Cidade,
                estado = filiado.Estado,
                responsavel = filiado.Responsavel,
                cobrancas
            };
        }

        public async Task<PagamentoAcademiaPixResponseDto> GerarPixPublicoAsync(PagamentoFiliadoPublicoDto dto)
        {
            var (cobranca, filiado, federacao) = ObterCobrancaPublica(dto.FederacaoId, dto.FiliadoId, dto.PagamentoId);

            if (dto.Valor <= 0 || decimal.Round(dto.Valor, 2) != decimal.Round(cobranca.Valor, 2))
            {
                throw new InvalidOperationException("O valor informado nao corresponde a cobranca.");
            }

            if (string.IsNullOrWhiteSpace(federacao.MercadoPagoAccessToken))
            {
                throw new InvalidOperationException("A federacao ainda nao configurou o token do Mercado Pago para PIX.");
            }

            var result = await _mercadoPagoGateway.CriarPagamentoPixAsync(
                cobranca.Valor,
                cobranca.Descricao ?? $"Cobranca - {filiado.Nome}",
                !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email! : filiado.Email ?? "pagador@marcialprox.com.br",
                federacao.MercadoPagoAccessToken);

            cobranca.ExternalId = result.ExternalId;
            cobranca.FormaPagamentoNome = "PIX";
            cobranca.Status = MapearStatusGateway(result.Status);
            if (cobranca.Status == "Pago")
            {
                cobranca.DataPagamento = DateTime.UtcNow;
            }

            _context.SaveChanges();

            return new PagamentoAcademiaPixResponseDto
            {
                PagamentoId = cobranca.Id,
                Status = cobranca.Status,
                Payload = result.Payload,
                QrCodeBase64 = result.QrCodeBase64,
                ExternalId = result.ExternalId,
                VerificacaoAutomaticaDisponivel = true,
                Mensagem = cobranca.Status == "Pago"
                    ? "Pagamento PIX aprovado."
                    : "Cobranca PIX gerada. Aguarde a confirmacao do pagamento."
            };
        }

        public async Task<PagamentoAcademiaStatusDto> PagarCartaoPublicoAsync(PagamentoFiliadoCartaoPublicoDto dto)
        {
            var (cobranca, filiado, federacao) = ObterCobrancaPublica(dto.FederacaoId, dto.FiliadoId, dto.PagamentoId);

            if (dto.Valor <= 0 || decimal.Round(dto.Valor, 2) != decimal.Round(cobranca.Valor, 2))
            {
                throw new InvalidOperationException("O valor informado nao corresponde a cobranca.");
            }

            if (string.IsNullOrWhiteSpace(federacao.MercadoPagoAccessToken))
            {
                throw new InvalidOperationException("A federacao ainda nao configurou o token de recebimento do cartao.");
            }

            var gatewayDto = new PagamentoCartaoPublicoDto
            {
                Valor = cobranca.Valor,
                Parcelas = dto.Parcelas <= 0 ? 1 : dto.Parcelas,
                PayerEmail = string.IsNullOrWhiteSpace(dto.PayerEmail) ? filiado.Email : dto.PayerEmail,
                PayerCpf = dto.PayerCpf,
                CardToken = dto.CardToken,
                PaymentMethodId = dto.PaymentMethodId
            };

            var result = await _mercadoPagoGateway.ProcessarPagamentoCartaoAsync(
                gatewayDto,
                cobranca.Descricao ?? $"Cobranca - {filiado.Nome}",
                federacao.MercadoPagoAccessToken);

            cobranca.ExternalId = result.ExternalId;
            cobranca.FormaPagamentoNome = "Cartao";
            cobranca.Status = MapearStatusGateway(result.Status);

            if (cobranca.Status == "Pago")
            {
                cobranca.DataPagamento = DateTime.UtcNow;
            }

            _context.SaveChanges();
            return MapToStatus(cobranca);
        }

        public PagamentoAcademiaStatusDto AtualizarStatusPublico(int federacaoId, int pagamentoId)
        {
            var federacao = ObterFederacaoPublica(federacaoId);
            var ownerUserId = federacao.OwnerUserId
                ?? throw new InvalidOperationException("Federacao sem usuario responsavel configurado.");

            var cobranca = _context.PagamentosFiliados.FirstOrDefault(p =>
                p.Id == pagamentoId &&
                p.Filiado != null &&
                p.Filiado.OwnerUserId == ownerUserId);

            if (cobranca == null)
            {
                throw new InvalidOperationException("Cobranca nao encontrada.");
            }

            if (!string.IsNullOrWhiteSpace(cobranca.ExternalId) &&
                !string.IsNullOrWhiteSpace(federacao.MercadoPagoAccessToken))
            {
                var result = _mercadoPagoGateway.ConsultarPagamentoAsync(cobranca.ExternalId, federacao.MercadoPagoAccessToken)
                    .GetAwaiter()
                    .GetResult();

                cobranca.Status = MapearStatusGateway(result.Status);
                if (cobranca.Status == "Pago" && !cobranca.DataPagamento.HasValue)
                {
                    cobranca.DataPagamento = DateTime.UtcNow;
                }

                _context.SaveChanges();
            }

            return MapToStatus(cobranca);
        }

        public void Baixar(int id, string role, int? userId)
        {
            var query = _context.PagamentosFiliados.Where(p => p.Id == id);
            if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario invalido para baixar cobranca.");
                query = query.Where(p => p.Filiado!.OwnerUserId == userId.Value);
            }
            else if (!RoleScope.IsSuperAdmin(role))
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para baixar cobranca.");
            }

            var cobranca = query.FirstOrDefault();
            if (cobranca == null) throw new InvalidOperationException("Cobranca nao encontrada.");

            cobranca.Status = "Pago";
            cobranca.DataPagamento = DateTime.UtcNow;
            _context.SaveChanges();
        }

        private PagamentoFiliado ObterCobrancaPorEscopo(int id, string role, int? filiadoIdUsuario, int? userId)
        {
            var query = _context.PagamentosFiliados.Where(p => p.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario invalido para acessar cobranca.");
                query = query.Where(p => p.Filiado!.OwnerUserId == userId.Value);
            }
            else
            {
                if (!filiadoIdUsuario.HasValue) throw new UnauthorizedAccessException("Usuario sem vinculo com federacao.");
                query = query.Where(p => p.FiliadoId == filiadoIdUsuario.Value);
            }

            return query.FirstOrDefault() ?? throw new InvalidOperationException("Cobranca nao encontrada.");
        }

        private Federacao ObterFederacaoPublica(int federacaoId)
        {
            var federacao = _context.Federacoes.FirstOrDefault(f => f.Id == federacaoId && f.Ativo);
            return federacao ?? throw new InvalidOperationException("Federacao nao encontrada.");
        }

        private (PagamentoFiliado cobranca, Filiados filiado, Federacao federacao) ObterCobrancaPublica(
            int federacaoId,
            int filiadoId,
            int pagamentoId)
        {
            var federacao = ObterFederacaoPublica(federacaoId);
            var ownerUserId = federacao.OwnerUserId
                ?? throw new InvalidOperationException("Federacao sem usuario responsavel configurado.");

            var cobranca = _context.PagamentosFiliados.FirstOrDefault(p =>
                p.Id == pagamentoId &&
                p.FiliadoId == filiadoId &&
                p.Filiado != null &&
                p.Filiado.OwnerUserId == ownerUserId &&
                p.Status != "Pago");

            if (cobranca == null)
            {
                throw new InvalidOperationException("Cobranca nao encontrada ou ja paga.");
            }

            var filiado = cobranca.Filiado ?? _context.Filiado.First(f => f.Id == filiadoId);
            return (cobranca, filiado, federacao);
        }

        private string? ObterAccessTokenFederacaoPorUsuarioId(int? ownerUserId)
        {
            if (!ownerUserId.HasValue)
            {
                return null;
            }

            return _context.Federacoes
                .Where(f => f.OwnerUserId == ownerUserId.Value && f.Ativo)
                .Select(f => f.MercadoPagoAccessToken)
                .FirstOrDefault();
        }

        private static PagamentoAcademiaStatusDto MapToStatus(PagamentoFiliado pagamento)
        {
            return new PagamentoAcademiaStatusDto
            {
                PagamentoId = pagamento.Id,
                Status = pagamento.Status,
                FormaPagamentoNome = pagamento.FormaPagamentoNome,
                DataPagamento = pagamento.DataPagamento
            };
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

        private static string ApenasDigitos(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return new string(valor.Where(char.IsDigit).ToArray());
        }

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            return texto.Trim().ToLowerInvariant()
                .Replace("á", "a")
                .Replace("à", "a")
                .Replace("ã", "a")
                .Replace("â", "a")
                .Replace("é", "e")
                .Replace("ê", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ô", "o")
                .Replace("õ", "o")
                .Replace("ú", "u")
                .Replace("ç", "c");
        }
    }
}
