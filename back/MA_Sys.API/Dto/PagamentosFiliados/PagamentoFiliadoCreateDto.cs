namespace MA_Sys.API.Dto.PagamentosFiliados
{
    public class PagamentoFiliadoCreateDto
    {
        public int FiliadoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string? Descricao { get; set; }
    }
}
