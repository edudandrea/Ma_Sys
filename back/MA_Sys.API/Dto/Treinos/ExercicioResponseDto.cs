namespace MA_Sys.API.Dto.Treinos
{
    public class ExercicioResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? GrupoMuscular { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
    }
}
