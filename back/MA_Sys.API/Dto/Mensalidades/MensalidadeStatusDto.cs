namespace MA_Sys.API.Dto.Mensalidades
{
    public class MensalidadeStatusDto
    {
        public string Status { get; set; } = "Em dia";
        public DateTime DataVencimento { get; set; }
        public int DiasParaVencimento { get; set; }
        public string? UltimoPagamentoStatus { get; set; }
    }
}
