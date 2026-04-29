namespace MA_Sys.API.Dto.PagamentosAcademias
{
    public class PagamentoAcademiaCartaoDto
    {
        public string PayerEmail { get; set; } = string.Empty;
        public string CardToken { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = "visa";
        public int Parcelas { get; set; } = 1;
    }
}
