namespace MA_Sys.API.Dto.AcademiasDto
{
    public class AcademiaUpdateDto
    {
        public string? Nome { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? Cidade { get; set; }
        public string? Estado  { get; set; }
        public string? LogoUrl { get; set; }
        public string? RedeSocial { get; set; }
        public string? Responsavel { get; set; }
        public string? MercadoPagoPublicKey { get; set; }
        public string? MercadoPagoAccessToken { get; set; }
    }
}
