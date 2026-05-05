namespace MA_Sys.API.Dto.Dashboard
{
    public class FiliadoCobrancaAlertaDto
    {
        public int PagamentoId { get; set; }
        public int FiliadoId { get; set; }
        public string? NomeFiliado { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public int DiasParaVencimento { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }
}
