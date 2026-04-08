namespace MA_Sys.API.Dto.ProfessoresDto
{
    public class ProfessorFiltroDto
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public string? Graduacao { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public bool Ativo { get; set; }
    }
}