namespace MA_Sys.API.Dto.PagamentosFiliados
{
    public class PagamentoFiliadoCartaoPublicoDto : PagamentoFiliadoPublicoDto
    {
        public int Parcelas { get; set; } = 1;
        public string? PayerEmail { get; set; }
        public string? PayerCpf { get; set; }
        public string? CardToken { get; set; }
        public string? PaymentMethodId { get; set; }
    }
}
