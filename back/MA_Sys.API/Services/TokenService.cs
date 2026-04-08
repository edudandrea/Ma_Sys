using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MA_SYS.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace MA_Sys.API.Services
{
    public class TokenService
    {
        public string GenerateToken(Users user)
        {

            Console.WriteLine($"Gerando token para usuário: {user.Login}, AcademiaId: {user.AcademiaId}, UserId: {user.UserId}, Role: {user.Role}");
            var key = Encoding.ASCII.GetBytes("Hco?JH8I=KtBT1dwsZGs]@/h/ga)5n7E]-QcQr_XT)}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login ?? ""),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("role", user.Role ?? "Academia"),
                new Claim(ClaimTypes.Role, user.Role ?? "Academia"),
                
            };

            if (user.AcademiaId.HasValue)
            {
                claims.Add(new Claim("AcademiaId", user.AcademiaId.Value.ToString()));
            }

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