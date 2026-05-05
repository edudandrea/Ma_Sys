using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.AcademiasDto;
using MA_Sys.API.Security;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class AcademiaService
    {
        private readonly IAcademiaRepository _repo;

        public AcademiaService(IAcademiaRepository repo)
        {
            _repo = repo;
        }

        public string GerarSlug(string nome)
        {
            return nome.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(";", "")
                .Replace(":", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("&", "")
                .Replace("*", "")
                .Replace("(", "")
                .Replace(")", "");
        }

        public List<AcademiaResponseDto> List(string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar academias.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(a => a.Id == academiaId);
            }

            return query.Select(a => new AcademiaResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Email = a.Email,
                Telefone = a.Telefone,
                LogoUrl = a.LogoUrl,
                Cidade = a.Cidade,
                RedeSocial = a.RedeSocial,
                Responsavel = a.Responsavel,
                MercadoPagoPublicKey = a.MercadoPagoPublicKey,
                Ativo = a.Ativo,
                totalAlunos = a.Alunos != null ? a.Alunos.Count() : 0,
                totalProfessores = a.Professores != null ? a.Professores.Count() : 0,
                Slug = a.Slug
            }).ToList();
        }

        public List<AcademiaResponseDto> Get(string role, AcademiaFiltroDto filtro, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar academias.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(a => a.Id == academiaId);
            }

            if (filtro.Id.HasValue)
                query = query.Where(a => a.Id == filtro.Id);

            if (!string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(a => a.Nome != null && a.Nome.Contains(filtro.Nome));

            return query.Select(a => new AcademiaResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Telefone = a.Telefone,
                LogoUrl = a.LogoUrl,
                Cidade = a.Cidade,
                RedeSocial = a.RedeSocial,
                Responsavel = a.Responsavel,
                MercadoPagoPublicKey = a.MercadoPagoPublicKey,
                Ativo = a.Ativo,
                totalAlunos = a.Alunos != null ? a.Alunos.Count() : 0,
                totalProfessores = a.Professores != null ? a.Professores.Count() : 0,
                Slug = a.Slug
            }).ToList();
        }

        public AcademiaDto? GetById(int id, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar academia.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.Id == academiaId);
            }

            return query.Select(a => new AcademiaDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Telefone = a.Telefone,
                Email = a.Email,
                LogoUrl = a.LogoUrl,
                Cidade = a.Cidade,
                Estado = a.Estado,
                DataCadastro = a.DataCadastro,
                Slug = a.Slug,
                MercadoPagoPublicKey = a.MercadoPagoPublicKey,
                MercadoPagoAccessToken = a.MercadoPagoAccessToken,
                Ativo = a.Ativo
            }).FirstOrDefault();
        }

        public void Add(AcademiaCreateDto dto, string? role, int? currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new InvalidOperationException("Nome da academia e obrigatorio.");

            var slugBase = GerarSlug(dto.Nome);
            var slug = slugBase;
            var count = 1;

            while (_repo.Query().Any(a => a.Slug == slug))
            {
                slug = $"{slugBase}-{count}";
                count++;
            }

            var academia = new Academia
            {
                Nome = dto.Nome,
                Slug = slug,
                Telefone = dto.Telefone,
                Email = dto.Email,
                LogoUrl = dto.LogoUrl?.Trim(),
                MercadoPagoPublicKey = dto.MercadoPagoPublicKey?.Trim(),
                MercadoPagoAccessToken = dto.MercadoPagoAccessToken?.Trim(),
                OwnerUserId = RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role) ? currentUserId : dto.OwnerUserId,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _repo.Add(academia);
            _repo.Save();
        }

        public void Update(int id, AcademiaUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar academia.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.Id == academiaId);
            }

            var academia = query.FirstOrDefault();

            if (academia == null)
                throw new Exception("Academia nao encontrada");

            academia.Nome = dto.Nome?.Trim();
            academia.Telefone = dto.Telefone;
            academia.Cidade = dto.Cidade;
            academia.Estado = dto.Estado;
            academia.LogoUrl = dto.LogoUrl?.Trim();
            academia.RedeSocial = dto.RedeSocial;
            academia.Email = dto.Email;
            academia.Responsavel = dto.Responsavel;
            academia.MercadoPagoPublicKey = dto.MercadoPagoPublicKey?.Trim();
            academia.MercadoPagoAccessToken = dto.MercadoPagoAccessToken?.Trim();

            _repo.Save();
        }

        public void Delete(int id, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para excluir academia.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.Id == academiaId);
            }

            var academia = query.FirstOrDefault();

            if (academia == null)
                throw new Exception("Academia nao encontrada");

            _repo.Delete(academia);
            _repo.Save();
        }

        public void UpdateStatus(int id, string role, int? academiaId, int? userId, bool ativo)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar status da academia.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.Id == academiaId);
            }

            var academia = query.FirstOrDefault();

            if (academia == null)
                throw new Exception("Academia nao encontrada");

            academia.Ativo = ativo;

            _repo.Update(academia);
            _repo.Save();
        }
    }
}
