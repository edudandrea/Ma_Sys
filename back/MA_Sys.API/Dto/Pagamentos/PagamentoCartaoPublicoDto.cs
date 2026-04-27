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
        public CartaoDto Cartao { get; set; } = new();
    }

    public class CartaoDto
    {
        public string Numero { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Validade { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
    }
}
