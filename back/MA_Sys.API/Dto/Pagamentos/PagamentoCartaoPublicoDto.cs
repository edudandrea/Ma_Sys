namespace MA_Sys.API.Dto.Pagamentos
{
    public class PagamentoCartaoPublicoDto
    {
        public string Slug { get; set; } = string.Empty;
        public int AlunoId { get; set; }
        public int MatriculaId { get; set; }
        public int PlanoId { get; set; }
        public int FormaPagamentoId { get; set; }
        public decimal Valor { get; set; }
        public int Parcelas { get; set; } = 1;
        public string PayerEmail { get; set; } = string.Empty;
        public string PayerCpf { get; set; } = string.Empty;
        public string CardToken { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = "visa";
    }
}
