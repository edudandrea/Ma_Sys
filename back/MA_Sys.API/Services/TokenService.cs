using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MA_SYS.Api.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MA_Sys.API.Services
{
    public class TokenService
    {
        private readonly byte[] _key;
        private readonly string? _issuer;
        private readonly string? _audience;

        public TokenService(IConfiguration configuration, IHostEnvironment environment)
        {
            var jwtKey = configuration["Jwt:Key"];
            var jwtPlaceholder = string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Contains("__CONFIGURE_VIA_", StringComparison.Ordinal);

            if (jwtPlaceholder && environment.IsDevelopment())
            {
                jwtKey = "dev-only-jwt-key-marcia-prox-local-2026";
            }

            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Contains("__CONFIGURE_VIA_", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Jwt:Key nao configurada. Defina a chave real via variavel de ambiente Jwt__Key.");
            }

            _key = Encoding.ASCII.GetBytes(jwtKey);
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        public string GenerateToken(Users user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login ?? string.Empty),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "Academia"),
            };

            if (user.AcademiaId.HasValue)
            {
                claims.Add(new Claim("AcademiaId", user.AcademiaId.Value.ToString()));
            }

            if (user.FederacaoId.HasValue)
            {
                claims.Add(new Claim("FederacaoId", user.FederacaoId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
