using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MA_Sys.API.Dto;
using MA_Sys.API.Models;
using MA_Sys.API.Services;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly TokenService _tokenService;

        public UserController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            try
            {
                if (userLogin == null)
                    return BadRequest("Corpo da requisição inválido");

                var user = _context.User.FirstOrDefault(c => c.Login == userLogin.Login);
                if (user == null)
                    return NotFound("Usuário não encontrado");

                if (string.IsNullOrEmpty(user.Password))
                    return StatusCode(500, "Senha armazenada está vazia ou nula");

                var hasher = new PasswordHasher<Users>();
                var result = hasher.VerifyHashedPassword(user, user.Password, userLogin.Password);
                if (result == PasswordVerificationResult.Failed)
                    return BadRequest("Senha inválida");

                if (user.Function == null)
                    return StatusCode(500, "Função (role) do usuário está nula");

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Role, user.Function.ToString())
                };

                var token = _tokenService.CreateToken(user, claims);

                user.Password = "";

                var response = new UserResponse
                {
                    Users = user,
                    Token = token
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateUser([FromBody] Users user)
        {
            var userExist = await _context.User.FirstOrDefaultAsync(c => c.Login == user.Login);
            if (userExist != null)
            {
                return BadRequest("Usuário já existe.");
            }

            var hasher = new PasswordHasher<Users>();
            var hashedPassword = hasher.HashPassword(user, user.Password);

            var newUser = new Users
            {
                Login = user.Login,
                UserName = user.UserName,
                Password = hashedPassword,  // Armazena o hash da senha
                Function = user.Function ?? "user",  // Caso não tenha função, atribui "user"
            };
            
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            // Garantir que o usuário foi salvo antes de gerar o token
            var userFromDb = await _context.User.FirstOrDefaultAsync(c => c.Login == newUser.Login);
            if (userFromDb == null)
            {
                return StatusCode(500, "Erro ao salvar o usuário no banco de dados.");
            }

            var token = _tokenService.CreateToken(user, new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Function)  // Inclui a role no token
            });

            user.Password = "";  // Não envia a senha no retorno

            var result = new UserResponse
            {
                Users = user,
                Token = token
            };

            return Ok(result);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] Users user)
        {
            // Buscar o usuário pelo ID
            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingUser == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            // Manter o login do usuário, pois ele não pode ser alterado
            user.Login = existingUser.Login;

            // Se a senha for fornecida, recrie a senha criptografada
            if (!string.IsNullOrEmpty(user.Password))
            {
                var hasher = new PasswordHasher<Users>();
                user.Password = hasher.HashPassword(user, user.Password); // Criptografar a senha
            }

            // Atualiza os dados do usuário
            existingUser.UserName = user.UserName ?? existingUser.UserName;
            existingUser.Password = user.Password ?? existingUser.Password;
            existingUser.Function = user.Function ?? existingUser.Function;

            // Atualiza o usuário no banco de dados
            _context.User.Update(existingUser);
            await _context.SaveChangesAsync();

            // Gerar um novo token JWT usando o TokenServices
            var claims = new[] // Caso tenha claims extras, adicione-as aqui
            {
                new Claim(ClaimTypes.Name, existingUser.UserName),
                new Claim(ClaimTypes.Role, existingUser.Function),
            };

            var token = _tokenService.CreateToken(existingUser, claims); // Gerando o token

            // Retorna o usuário atualizado e o novo token
            var result = new UserResponse
            {
                Users = existingUser,
                Token = token
            };

            return Ok(result);
        }
    }
}