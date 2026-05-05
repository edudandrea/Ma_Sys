using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Planos;
using MA_Sys.API.Security;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class PlanosService
    {
        private readonly IPlanosRepository _repo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IAcademiaRepository _academiaRepo;

        public PlanosService(IPlanosRepository repo, IAlunoRepository alunoRepo, IAcademiaRepository academiaRepo)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
            _academiaRepo = academiaRepo;
        }

        public List<PlanosResponseDto> List(string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar planos.");

                query = query.Where(pl => pl.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar planos.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(a => a.AcademiaId.HasValue && academiaIds.Contains(a.AcademiaId.Value));
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia.");

                query = query.Where(a => a.AcademiaId == academiaId);
            }

            return query.Select(a => new PlanosResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Valor = a.Valor,
                DuracaoMeses = a.DuracaoMeses,
                AcademiaId = a.AcademiaId,
                OwnerUserId = a.OwnerUserId,
                AcademiaNome = a.Academia != null ? a.Academia.Nome : null,
                Escopo = a.OwnerUserId.HasValue ? "Federacao" : "Academia",
                Ativo = a.Ativo
            }).ToList();
        }

        public List<PlanosResponseDto> Get(string role, PlanosFiltroDto filtro, int? academiaId, int? userId)
        {
            var query = _repo.Query().AsNoTracking();
            var isSuperAdmin = RoleScope.IsSuperAdmin(role);
            var isAdmin = RoleScope.IsAdmin(role);
            var isFederacao = RoleScope.IsFederacao(role);

            if (isSuperAdmin)
            {
            }
            else if (isFederacao)
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar planos.");

                query = query.Where(pl => pl.OwnerUserId == userId.Value);
            }
            else if (isAdmin)
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar planos.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(pl => pl.AcademiaId.HasValue && academiaIds.Contains(pl.AcademiaId.Value));
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar planos.");

                query = query.Where(pl => pl.AcademiaId == academiaId);
            }

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Nome))
                    query = query.Where(pl => pl.Nome.Contains(filtro.Nome));

                if (filtro.Ativo.HasValue)
                    query = query.Where(pl => pl.Ativo == filtro.Ativo.Value);
            }

            var academiaIdsUsuario = isAdmin && userId.HasValue
                ? _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList()
                : new List<int>();

            return query.Select(pl => new PlanosResponseDto
            {
                Id = pl.Id,
                Nome = pl.Nome,
                Valor = pl.Valor,
                DuracaoMeses = pl.DuracaoMeses,
                AcademiaId = pl.AcademiaId,
                OwnerUserId = pl.OwnerUserId,
                AcademiaNome = pl.Academia != null ? pl.Academia.Nome : null,
                Escopo = pl.OwnerUserId.HasValue ? "Federacao" : "Academia",
                Ativo = pl.Ativo,
                TotalAlunos = isFederacao || pl.OwnerUserId.HasValue
                    ? 0
                    : _alunoRepo.Query().Count(a =>
                        a.PlanoId == pl.Id &&
                        (isSuperAdmin || a.AcademiaId == academiaId || (isAdmin && academiaIdsUsuario.Contains(a.AcademiaId))))
            }).ToList();
        }

        public void Add(PlanosCreateDto dto, string role, int? academiaId, int? userId)
        {
            int? academiaDestino = RoleScope.IsAcademia(role) ? (academiaId ?? 0) : dto.AcademiaId;
            int? ownerUserId = null;

            if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para cadastrar plano.");

                academiaDestino = null;
                ownerUserId = userId.Value;
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para cadastrar plano.");

                if (!academiaDestino.HasValue)
                    throw new InvalidOperationException("Informe uma academia para cadastrar o plano.");

                var academiaPertenceAoUsuario = _academiaRepo.Query()
                    .Any(a => a.Id == academiaDestino.Value && a.OwnerUserId == userId.Value);

                if (!academiaPertenceAoUsuario)
                    throw new UnauthorizedAccessException("Usuario nao pode cadastrar plano em academia fora do seu escopo.");
            }
            else if (!RoleScope.IsSuperAdmin(role) && !academiaDestino.HasValue)
            {
                throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode cadastrar plano.");
            }
            else if (RoleScope.IsSuperAdmin(role) && !academiaDestino.HasValue)
            {
                throw new InvalidOperationException("Informe uma academia para cadastrar o plano.");
            }

            var plano = new Plano
            {
                Nome = dto.Nome,
                Valor = dto.Valor,
                DuracaoMeses = dto.DuracaoMeses,
                AcademiaId = academiaDestino,
                OwnerUserId = ownerUserId,
                Ativo = true
            };

            _repo.Add(plano);
            _repo.Save();
        }

        public void Update(int id, PlanosUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar plano.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar plano.");

                query = query.Where(a => a.Academia != null && a.Academia.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var plano = query.FirstOrDefault();

            if (plano == null)
                throw new Exception("Plano nao encontrado");

            plano.Nome = dto.Nome?.Trim();
            plano.Ativo = dto.Ativo;
            plano.Valor = dto.Valor;
            plano.DuracaoMeses = dto.DuracaoMeses;

            _repo.Save();
        }

        public void UpdateStatus(int id, string role, int? academiaId, int? userId, bool ativo)
        {
            var query = _repo.Query().Where(p => p.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar status do plano.");

                query = query.Where(p => p.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar status do plano.");

                query = query.Where(p => p.Academia != null && p.Academia.OwnerUserId == userId.Value);
            }
            else
            {
                query = query.Where(p => p.AcademiaId == academiaId);
            }

            var plano = query.FirstOrDefault();

            if (plano == null)
                throw new Exception("Plano nao encontrado");

            plano.Ativo = ativo;

            _repo.Update(plano);
            _repo.Save();
        }

        public PlanosDto GetTotalAlunos(int? academiaId, int? planoId, string? role, int? userId)
        {
            var query = _alunoRepo.Query().Where(a => a.PlanoId == planoId);
            var isSuperAdmin = RoleScope.IsSuperAdmin(role);
            var isAdmin = RoleScope.IsAdmin(role);
            var isFederacao = RoleScope.IsFederacao(role);

            if (isFederacao)
            {
                query = query.Where(_ => false);
            }
            else if (!isSuperAdmin && !isAdmin && academiaId.HasValue)
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }
            else if (isAdmin && userId.HasValue)
            {
                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList();

                query = query.Where(a => academiaIds.Contains(a.AcademiaId));
            }

            return new PlanosDto
            {
                TotalAlunos = query.Count()
            };
        }
    }
}
