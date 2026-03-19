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

        public string CreateToken (Users users, Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Key").Get<string>());

            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, users.UserName.ToString()),  // Nome do usuário
                  // Função/Role do usuário
            });

            // Incluindo os claims adicionais recebidos como parâmetro
            if (claims != null && claims.Length > 0)
            {
                claimsIdentity.AddClaims(claims);  // Adiciona os claims extras
            }


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                    Subject = claimsIdentity,
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}