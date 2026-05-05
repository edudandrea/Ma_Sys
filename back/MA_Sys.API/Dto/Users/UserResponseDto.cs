namespace MA_Sys.API.Dto.UsersDto
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string? Login { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? AcademiaId { get; set; }    
        public string? AcademiaNome { get; set; }    
        public int? FederacaoId { get; set; }
        public string? FederacaoNome { get; set; }
        
    }
}
