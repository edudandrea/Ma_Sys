namespace MA_Sys.API.Dto.MensalidadesSistema
{
    public class MensalidadeSistemaResponseDto
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public int PrazoPagamentoDias { get; set; }
        public int MesesUso { get; set; }
        public bool Ativo { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
