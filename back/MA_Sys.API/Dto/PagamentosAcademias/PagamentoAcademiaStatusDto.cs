namespace MA_Sys.API.Dto.PagamentosAcademias
{
    public class PagamentoAcademiaStatusDto
    {
        public int PagamentoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? FormaPagamentoNome { get; set; }
        public DateTime? DataPagamento { get; set; }
    }
}
