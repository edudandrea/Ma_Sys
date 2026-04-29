namespace MA_Sys.API.Dto.PagamentosAcademias
{
    public class PagamentoAcademiaResponseDto
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string NomeAcademia { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? FormaPagamentoNome { get; set; }
        public int? MensalidadeSistemaId { get; set; }
    }
}
