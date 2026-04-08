using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.UsersDto;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public List<UserResponseDto> List()
        {
            var user = _repo.Query();

            return user.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Login = u.Login,
                Email = u.Email,
                Password = u.Password,
                Role = u.Role,
                
            }).ToList();
        }

        public List<UserResponseDto> Get(UserFiltroDto filtro)
        {
            var query = _repo.Query();          

            
            if (!string.IsNullOrEmpty(filtro.UserName))
                query = query.Where(u => u.UserName.Contains(filtro.UserName));

            if (!string.IsNullOrEmpty(filtro.Role))
                query = query.Where(u => u.Role.Contains(filtro.Role));

            return query.Select(u => new UserResponseDto
            {                
                UserName = u.UserName,
                Email = u.Email,
                Password = u.Password,
                Role = u.Role,
                AcademiaId = u.AcademiaId
                
            }).ToList();
           
        }

        public void Add(UserCreateDto dto)
        {
            var user = new Users
            {
                UserName = dto.UserName,
                Login = dto.Login,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                AcademiaId = dto.Role == "Admin" ? null : dto.AcademiaId
            };

            _repo.Add(user);
            _repo.Save();
        }

        public void Update(int id, UserUpdateDto dto)
        {
            var user = _repo.Query()
                        .FirstOrDefault(a => a.UserId == id);

            if (user == null)
                throw new Exception("Usuário não encontrado");

            user.UserName = dto.UserName?.Trim();
            user.Email = dto.Email?.Trim();
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.Role = dto.Role?.Trim();
            user.AcademiaId = dto.AcademiaId;

             _repo.Update(user);
        
            _repo.Save();
        }

        public void Delete(int userId, int academiaId)
        {
            var user = _repo.GetById(userId, academiaId);

            if (user == null)
                throw new Exception("usuário não encontrado");

            _repo.Delete(user);
            _repo.Save();
        }

    }
}