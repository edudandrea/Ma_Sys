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

        public List<UserResponseDto> ListUser()
        {
            var user = _repo.Query();

            return user.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Login = u.Login,
                Email = u.Email,
                Password = u.Password,
                Role = u.Role,
                Function = u.Function

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
                Function = dto.Function,
                AcademiaId = dto.AcademiaId,
            };

            _repo.Add(user);
            _repo.Save();
        }

    }
}