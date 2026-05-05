namespace MA_Sys.API.Dto.PagamentosFiliados
{
    public class PagamentoFiliadoPublicoDto
    {
        public int FederacaoId { get; set; }
        public int FiliadoId { get; set; }
        public int PagamentoId { get; set; }
        public decimal Valor { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Cidade { get; set; }
    }
}
