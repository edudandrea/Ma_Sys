namespace MA_Sys.API.Dto.Federacoes
{
    public class FederacaoCreateUpdateDto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Responsavel { get; set; }
        public string? RedeSocial { get; set; }
        public string? LogoUrl { get; set; }
        public int? OwnerUserId { get; set; }
        public bool Ativo { get; set; }
         public string? MercadoPagoPublicKey { get; set; }
        public string? MercadoPagoAccessToken { get; set; }
    }
}
