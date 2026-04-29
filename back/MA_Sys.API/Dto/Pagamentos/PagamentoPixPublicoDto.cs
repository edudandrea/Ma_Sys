namespace MA_Sys.API.Dto.Pagamentos
{
    public class PagamentoPixPublicoDto
    {
        public string Slug { get; set; } = string.Empty;
        public int AlunoId { get; set; }
        public int MatriculaId { get; set; }
        public int PlanoId { get; set; }
        public int FormaPagamentoId { get; set; }
        public decimal Valor { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Cidade { get; set; }
    }
}
