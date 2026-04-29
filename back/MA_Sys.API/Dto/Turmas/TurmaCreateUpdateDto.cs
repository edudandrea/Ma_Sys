namespace MA_Sys.API.Dto.Turmas
{
    public class TurmaCreateUpdateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public List<string> DiasSemana { get; set; } = [];
        public List<int> AlunoIds { get; set; } = [];
        public bool Ativo { get; set; } = true;
    }
}
