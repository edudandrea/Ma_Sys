using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto;
using MA_Sys.API.Dto.UsersDto;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : BaseController
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
            var (role, academiaId, userId) = GetUserInfo();
            var user = _service.List(role, academiaId, userId);
            return Ok(user);
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] UserFiltroDto filtro)
        {
            var (role, academiaId, userId) = GetUserInfo();
            var user = _service.Get(role, academiaId, userId, filtro);

            return Ok(user);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            var user = _service.GetById(id, role, academiaId, userId);
            return Ok(user);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Login e senha sao obrigatorios.");

            var user = _userRepo.GetByLogin(dto.Login.Trim());

            if (user == null)
                return Unauthorized("Usuario nao encontrado");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Senha invalida");

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                token,
                usuario = new
                {
                    id = user.UserId,
                    userName = user.UserName,
                    email = user.Email,
                    login = user.Login,
                    role = user.Role,
                    academiaId = user.AcademiaId,
                    federacaoId = user.FederacaoId,
                    academiaNome = user.Academia != null ? user.Academia.Nome : null,
                    federacaoNome = user.Federacao != null ? user.Federacao.Nome : null,
                    academiaLogoUrl = user.Academia != null ? user.Academia.LogoUrl : null,
                    federacaoLogoUrl = user.Federacao != null ? user.Federacao.LogoUrl : null
                }
            });
        }

        [HttpGet("bootstrap-status")]
        [AllowAnonymous]
        public IActionResult GetBootstrapStatus()
        {
            return Ok(new
            {
                requiresBootstrap = !_userRepo.HasAnyUser()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add([FromBody] UserCreateDto dto)
        {
            var bootstrapMode = !_userRepo.HasAnyUser();

            if (!bootstrapMode && !(User.Identity?.IsAuthenticated ?? false))
            {
                return Unauthorized();
            }

            if (bootstrapMode)
            {
                dto.Role = "SuperAdmin";
                dto.AcademiaId = null;
            }
            else
            {
                var (role, academiaId, userId) = GetUserInfo();
                if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role))
                {
                    return Forbid();
                }

                if (string.Equals(dto.Role, "Academia", StringComparison.OrdinalIgnoreCase))
                {
                    dto.AcademiaId = dto.AcademiaId ?? academiaId;
                }

                _service.Add(dto, userId);
                return NoContent();
            }

            _service.Add(dto, null);
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UserUpdateDto dto)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Update(id, dto, role, academiaId, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Delete(id, role, academiaId, userId);
            return NoContent();
        }
    }
}
