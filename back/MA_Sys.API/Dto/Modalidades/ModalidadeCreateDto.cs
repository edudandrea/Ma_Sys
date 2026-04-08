namespace MA_Sys.API.Dto.ModalidadesDto
{
    public class ModalidadeCreateDto
    {
        public string? NomeModalidade { get; set; }
        public bool Ativo { get; set; }
        public int? AcademiaId { get; set; }
    }
}