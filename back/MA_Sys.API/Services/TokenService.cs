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
            var key = Encoding.ASCII.GetBytes("Rv6_wy.fMyEj•••••••••••••••••••(#9dbq!-3Gji");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Login ?? ""),
                new Claim("AcademiaId", user.AcademiaId.ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "Academia"),
                
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