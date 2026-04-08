namespace MA_Sys.API.Dto.ModalidadesDto
{
    public class ModalidadeResponseDto
    {
        public int? Id { get; set; }
        public string? NomeModalidade { get; set; }
        public int AcademiaId { get; set; }
        public bool Ativo { get; set; }

    }
}