namespace MA_Sys.API.Dto.FluxoCaixa
{
    public class FluxoCaixaMovimentoDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string AcademiaNome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? FormaPagamentoNome { get; set; }
    }
}
