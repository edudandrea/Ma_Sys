namespace MA_Sys.API.Dto.AcademiasDto
{
    public class AcademiaCreateDto
    {
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? LogoUrl { get; set; }
        public string? MercadoPagoPublicKey { get; set; }
        public string? MercadoPagoAccessToken { get; set; }
        public int? OwnerUserId { get; set; }
    }
}
