namespace MA_Sys.API.Dto.PagamentosAcademias
{
    public class PagamentoAcademiaCreateDto
    {
        public int AcademiaId { get; set; }
        public int? MensalidadeSistemaId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string? Descricao { get; set; }
    }
}
