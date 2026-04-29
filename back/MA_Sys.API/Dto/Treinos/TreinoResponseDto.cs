namespace MA_Sys.API.Dto.Treinos
{
    public class TreinoResponseDto
    {
        public int Id { get; set; }
        public int AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? Objetivo { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
        public List<TreinoExercicioDto> Exercicios { get; set; } = [];
    }
}
