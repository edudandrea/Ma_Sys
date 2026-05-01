namespace MA_Sys.API.Dto.Treinos
{
    public class TreinoCreateUpdateDto
    {
        public int AlunoId { get; set; }
        public int? ProfessorId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Objetivo { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; } = true;
        public List<TreinoExercicioDto> Exercicios { get; set; } = [];
    }
}
