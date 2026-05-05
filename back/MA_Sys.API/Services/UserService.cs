using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.UsersDto;
using MA_Sys.API.Security;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public List<UserResponseDto> List(string role, int? academiaId, int? userId)
        {
            IQueryable<Users> user = _repo.Query().Include(u => u.Academia).Include(u => u.Federacao);

            if (RoleScope.IsSuperAdmin(role))
            {
                return MapUsers(user);
            }

            if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                user = user.Where(u => u.UserId == userId.Value || u.CreatedByUserId == userId.Value);
                return MapUsers(user);
            }

            user = user.Where(u => u.AcademiaId == academiaId);
            return MapUsers(user);
        }

        public List<UserResponseDto> Get(string role, int? academiaId, int? userId, UserFiltroDto filtro)
        {
            filtro ??= new UserFiltroDto();
            IQueryable<Users> query = _repo.Query().Include(u => u.Academia).Include(u => u.Federacao);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(u => u.UserId == userId.Value || u.CreatedByUserId == userId.Value);
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar usuarios.");

                query = query.Where(u => u.AcademiaId == academiaId);
            }

            if (!string.IsNullOrEmpty(filtro.UserName))
                query = query.Where(u => u.UserName != null && u.UserName.Contains(filtro.UserName));

            if (!string.IsNullOrEmpty(filtro.Role))
                query = query.Where(u => u.Role != null && u.Role.Contains(filtro.Role));

            return MapUsers(query);
        }

        public UserResponseDto GetById(int id, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query()
                .Include(u => u.Academia)
                .Include(u => u.Federacao)
                .Where(u => u.UserId == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(u => u.UserId == userId.Value || u.CreatedByUserId == userId.Value);
            }
            else
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar usuarios.");

                query = query.Where(u => u.AcademiaId == academiaId.Value);
            }

            var user = query.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Login = u.Login,
                Email = u.Email,
                Role = u.Role,
                AcademiaId = u.AcademiaId,
                AcademiaNome = u.Academia != null ? u.Academia.Nome : null,
                FederacaoId = u.FederacaoId,
                FederacaoNome = u.Federacao != null ? u.Federacao.Nome : null
            }).FirstOrDefault();

            if (user == null)
                throw new KeyNotFoundException("Usuario nao encontrado.");

            return user;
        }

        public void Add(UserCreateDto dto, int? currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) ||
                string.IsNullOrWhiteSpace(dto.UserName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new InvalidOperationException("Nome, login, email e senha sao obrigatorios.");
            }

            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Academia" : dto.Role.Trim();

            var user = new Users
            {
                UserName = dto.UserName.Trim(),
                Login = dto.Login.Trim(),
                Email = dto.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role,
                AcademiaId = string.Equals(role, "Academia", StringComparison.OrdinalIgnoreCase) ? dto.AcademiaId : null,
                FederacaoId = string.Equals(role, "Federacao", StringComparison.OrdinalIgnoreCase) ? dto.FederacaoId : null,
                CreatedByUserId = currentUserId
            };

            _repo.Add(user);
            _repo.Save();
        }

        public void Update(int id, UserUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(u => u.UserId == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(u => u.CreatedByUserId == userId.Value || u.UserId == userId.Value);
            }
            else
            {
                query = query.Where(u => u.AcademiaId == academiaId);
            }

            var user = query.FirstOrDefault();

            if (user == null)
                throw new Exception("Usuario nao encontrado");

            if (!string.IsNullOrWhiteSpace(dto.UserName))
                user.UserName = dto.UserName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            if ((RoleScope.IsSuperAdmin(role) || RoleScope.IsAdmin(role)) && !string.IsNullOrWhiteSpace(dto.Role))
            {
                var novoRole = dto.Role.Trim();
                user.Role = novoRole;
                user.AcademiaId = string.Equals(novoRole, "Academia", StringComparison.OrdinalIgnoreCase) ? dto.AcademiaId : null;
                user.FederacaoId = string.Equals(novoRole, "Federacao", StringComparison.OrdinalIgnoreCase) ? dto.FederacaoId : null;
            }

            _repo.Update(user);
            _repo.Save();
        }

        public void Delete(int userId, string role, int? academiaId, int? currentUserId)
        {
            var query = _repo.Query().Where(u => u.UserId == userId);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!currentUserId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(u => u.CreatedByUserId == currentUserId.Value);
            }
            else
            {
                query = query.Where(u => u.AcademiaId == academiaId);
            }

            var user = query.FirstOrDefault();

            if (user == null)
                throw new Exception("Usuario nao encontrado");

            _repo.Delete(user);
            _repo.Save();
        }

        private static List<UserResponseDto> MapUsers(IQueryable<Users> query)
        {
            return query.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Login = u.Login,
                Email = u.Email,
                Role = u.Role,
                AcademiaId = u.AcademiaId,
                AcademiaNome = u.Academia != null ? u.Academia.Nome : null,
                FederacaoId = u.FederacaoId,
                FederacaoNome = u.Federacao != null ? u.Federacao.Nome : null
            }).ToList();
        }
    }
}
