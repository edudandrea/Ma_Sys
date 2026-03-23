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
        public IActionResult Login([FromBody] UserLoginDto dto)
        {
            var user = _context.User
                .FirstOrDefault(u => u.Email == dto.Login && u.Password == dto.Password);

            if (user == null)
                return Unauthorized("Usuário inválido");

            var token = _tokenService.GerarToken(user);

            return Ok(new { token });
        }

        
    }
}