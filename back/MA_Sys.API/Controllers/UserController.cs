using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto;
using MA_Sys.API.Dto.UsersDto;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly TokenService _tokenService;
        private readonly UserService _service;

        public UserController(IUserRepository userRepo, TokenService tokenService, UserService service)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _service = service;
        }

        [HttpGet]
        public IActionResult List()
        {
            var user = _service.List();
            return Ok(user);
        }



        [HttpGet("{id}")]
        public IActionResult Get([FromBody] UserFiltroDto filtro)
        {
            
            var user = _service.Get(filtro);

            return Ok(user);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserLoginDto dto)
        {
            var user = _userRepo.GetByLogin(dto.Login);

            if(user == null)
                return Unauthorized("Usuário não encontrado");
                

            if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Senha inválida");

            var token = _tokenService.GenerateToken(user);

            return Ok(new { 
                
                token = token,
                usuario = new
                {
                    id = user.UserId,
                    login = user.Login,
                    role = user.Role,
                    academiaId = user.AcademiaId
                }
                   
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add ([FromBody] UserCreateDto dto)
        {
            _service.Add(dto);
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UserUpdateDto dto)
        {
            _service.Update(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int userId)
        {
            _service.Delete(userId, 0); 
            return NoContent();
        }

    }
}