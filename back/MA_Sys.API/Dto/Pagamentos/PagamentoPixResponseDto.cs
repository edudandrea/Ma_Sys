namespace MA_Sys.API.Dto.Pagamentos
{
    public class PagamentoPixResponseDto
    {
        public int PagamentoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Payload { get; set; }
        public string? QrCodeBase64 { get; set; }
        public string? TicketUrl { get; set; }
        public string? ExternalId { get; set; }
        public string? StatusDetail { get; set; }
        public bool AmbienteTeste { get; set; }
        public bool VerificacaoAutomaticaDisponivel { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}
