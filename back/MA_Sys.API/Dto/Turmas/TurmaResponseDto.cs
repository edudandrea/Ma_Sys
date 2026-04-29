namespace MA_Sys.API.Dto.Turmas
{
    public class TurmaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public List<string> DiasSemana { get; set; } = [];
        public bool Ativo { get; set; }
        public List<TurmaAlunoDto> Alunos { get; set; } = [];
    }
}
