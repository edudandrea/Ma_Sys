namespace MA_Sys.API.Dto.PagamentosFiliados
{
    public class PagamentoFiliadoResponseDto
    {
        public int Id { get; set; }
        public int FiliadoId { get; set; }
        public string? NomeFiliado { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = "Pendente";
        public string? Descricao { get; set; }
        public string? FormaPagamentoNome { get; set; }
    }
}
