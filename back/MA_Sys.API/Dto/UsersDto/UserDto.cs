using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string? Login { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public int? AcademiaId { get; set; }
        public Academia? Academia { get; set; }
    }
}