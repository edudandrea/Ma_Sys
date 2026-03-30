namespace MA_Sys.API.Dto.UsersDto
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string? Login { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? Function { get; set; }
        
    }
}