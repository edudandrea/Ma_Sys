namespace MA_Sys.API.Dto.Treinos
{
    public class ExercicioCreateUpdateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string? GrupoMuscular { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
