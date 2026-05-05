using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.interfaces;
using MA_Sys.API.Dto.Federacao;
using MA_Sys.API.Dto.Filiados;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class FiliadosService
    {
        private readonly IFiliadosRepository _repo;

        public FiliadosService(IFiliadosRepository repo)
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

        public List<FiliadosResponseDto> List(string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar filiados.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(a => a.Id == academiaId);
            }

            return query.Select(a => new FiliadosResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Email = a.Email,
                Telefone = a.Telefone,
                LogoUrl = a.LogoUrl,
                Cidade = a.Cidade,
                RedeSocial = a.RedeSocial,
                Responsavel = a.Responsavel,
                Ativo = a.Ativo,
                Slug = a.Slug
            }).ToList();
        }

        public List<FiliadosResponseDto> Get(string role, FiliadosFiltroDto filtro, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar filiados.");

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

            return query.Select(a => new FiliadosResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Telefone = a.Telefone,
                LogoUrl = a.LogoUrl,
                Cidade = a.Cidade,
                RedeSocial = a.RedeSocial,
                Responsavel = a.Responsavel,
                Ativo = a.Ativo,
                Slug = a.Slug
            }).ToList();
        }

        public FiliadosDto? GetById(int id, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar filiado.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.Id == academiaId);
            }

            return query.Select(a => new FiliadosDto
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
                Ativo = a.Ativo
            }).FirstOrDefault();
        }

        public void Add(FiliadosCreateDto dto, string? role, int? currentUserId)
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

            var filiado = new Filiados
            {
                Nome = dto.Nome,
                Slug = slug,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Telefone = dto.Telefone,
                Email = dto.Email,
                LogoUrl = dto.LogoUrl?.Trim(),
                RedeSocial = dto.RedeSocial,
                Responsavel = dto.Responsavel,
                OwnerUserId = RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role) ? currentUserId : dto.OwnerUserId,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _repo.Add(filiado);
            _repo.Save();
        }

        public void Update(int id, FiliadosUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role) || RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar filiado.");

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
                    throw new UnauthorizedAccessException("Usuario invalido para excluir filiado.");

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
                    throw new UnauthorizedAccessException("Usuario invalido para alterar status do filiado.");

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
