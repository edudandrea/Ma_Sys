namespace MA_Sys.API.Dto.Treinos
{
    public class TreinoExercicioDto
    {
        public int ExercicioId { get; set; }
        public string? ExercicioNome { get; set; }
        public int Ordem { get; set; }
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public string? Descanso { get; set; }
        public string? Observacoes { get; set; }
    }
}
