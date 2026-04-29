namespace MA_Sys.API.Dto.PagamentosAcademias
{
    public class PagamentoAcademiaPixResponseDto
    {
        public int PagamentoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Payload { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? ExternalId { get; set; }
        public bool VerificacaoAutomaticaDisponivel { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}
