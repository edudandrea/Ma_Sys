using MA_Sys.API.Dto.Federacoes;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using MA_SYS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class FederacaoService
    {
        private readonly AppDbContext _context;

        public FederacaoService(AppDbContext context)
        {
            _context = context;
        }

        public List<FederacaoCreateUpdateDto> List(string role, int? userId, int? federacaoId)
        {
            var query = _context.Federacoes.AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
                query = query.Where(f => f.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!federacaoId.HasValue) throw new UnauthorizedAccessException("Usuario sem vinculo com federacao.");
                query = query.Where(f => f.Id == federacaoId.Value);
            }
            else
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para acessar federacoes.");
            }

            return query
                .OrderBy(f => f.Nome)
                .Select(f => new FederacaoCreateUpdateDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    Cidade = f.Cidade,
                    Estado = f.Estado,
                    Email = f.Email,
                    Telefone = f.Telefone,
                    Responsavel = f.Responsavel,
                    RedeSocial = f.RedeSocial,
                    LogoUrl = f.LogoUrl,
                    OwnerUserId = f.OwnerUserId,
                    Ativo = f.Ativo
                })
                .ToList();
        }

        public void Add(FederacaoCreateUpdateDto dto, string role, int? userId)
        {
            if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role))
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para criar federacoes.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                throw new InvalidOperationException("Nome da federacao e obrigatorio.");
            }

            var federacao = new Federacao
            {
                Nome = dto.Nome.Trim(),
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Responsavel = dto.Responsavel,
                RedeSocial = dto.RedeSocial,
                LogoUrl = dto.LogoUrl?.Trim(),
                MercadoPagoAccessToken = dto.MercadoPagoAccessToken,
                MercadoPagoPublicKey = dto.MercadoPagoPublicKey,
                OwnerUserId = RoleScope.IsAdmin(role) ? userId : dto.OwnerUserId,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _context.Federacoes.Add(federacao);
            _context.SaveChanges();
        }

        public void Update(int id, FederacaoCreateUpdateDto dto, string role, int? userId, int? federacaoId)
        {
            var query = _context.Federacoes.Where(f => f.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
                query = query.Where(f => f.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!federacaoId.HasValue) throw new UnauthorizedAccessException("Usuario sem vinculo com federacao.");
                query = query.Where(f => f.Id == federacaoId.Value);
            }
            else
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para editar federacoes.");
            }

            var federacao = query.FirstOrDefault() ?? throw new InvalidOperationException("Federacao nao encontrada.");

            federacao.Nome = dto.Nome?.Trim();
            federacao.Cidade = dto.Cidade;
            federacao.Estado = dto.Estado;
            federacao.Email = dto.Email;
            federacao.Telefone = dto.Telefone;
            federacao.Responsavel = dto.Responsavel;
            federacao.RedeSocial = dto.RedeSocial;
            federacao.LogoUrl = dto.LogoUrl?.Trim();

            _context.SaveChanges();
        }

        public void UpdateStatus(int id, bool ativo, string role, int? userId)
        {
            if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role))
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para alterar status.");
            }

            var query = _context.Federacoes.Where(f => f.Id == id);
            if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue) throw new UnauthorizedAccessException("Usuario administrador invalido.");
                query = query.Where(f => f.OwnerUserId == userId.Value);
            }

            var federacao = query.FirstOrDefault() ?? throw new InvalidOperationException("Federacao nao encontrada.");
            federacao.Ativo = ativo;
            _context.SaveChanges();
        }
    }
}
