using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MA_SYS.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace MA_Sys.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Users user)
        {
            var key = Encoding.ASCII.GetBytes("SUA_CHAVE_SECRETA_SUPER_FORTE");

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim("AcademiaId", user.AcademiaId.ToString()) // 🔥 ESSENCIAL
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}