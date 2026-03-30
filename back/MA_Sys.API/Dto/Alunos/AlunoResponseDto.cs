namespace MA_Sys.API.Dto.Alunos
{
    public class AlunoResponseDto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
        public string? Graduacao { get; set; }
    }
}