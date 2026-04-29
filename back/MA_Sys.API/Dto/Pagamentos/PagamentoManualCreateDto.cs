namespace MA_Sys.API.Dto.Pagamentos
{
    public class PagamentoManualCreateDto
    {
        public int AlunoId { get; set; }
        public int MatriculaId { get; set; }
        public int PlanoId { get; set; }
        public int FormaPagamentoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime? DataPagamento { get; set; }
    }
}
