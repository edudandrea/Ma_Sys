using MA_Sys.API.Dto.PagamentosAcademias;
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

            if (RoleScope.IsSuperAdmin(role) || RoleScope.IsFederacao(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
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
                 RoleScope.IsFederacao(role) ||
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

            var token = Environment.GetEnvironmentVariable("MERCADO_PAGO_ACCESS_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Configure MERCADO_PAGO_ACCESS_TOKEN para receber cobrancas de filiados.");
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

        public void Baixar(int id, string role, int? userId)
        {
            var query = _context.PagamentosFiliados.Where(p => p.Id == id);
            if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
                query = query.Where(p => p.Filiado!.OwnerUserId == userId.Value);
            }
            else if (!RoleScope.IsSuperAdmin(role) && !RoleScope.IsFederacao(role))
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

            if (RoleScope.IsSuperAdmin(role) || RoleScope.IsFederacao(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
                query = query.Where(p => p.Filiado!.OwnerUserId == userId.Value);
            }
            else
            {
                if (!filiadoIdUsuario.HasValue) throw new UnauthorizedAccessException("Usuario sem vinculo com federacao.");
                query = query.Where(p => p.FiliadoId == filiadoIdUsuario.Value);
            }

            return query.FirstOrDefault() ?? throw new InvalidOperationException("Cobranca nao encontrada.");
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
