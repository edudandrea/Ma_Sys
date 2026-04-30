namespace MA_Sys.API.Dto.AcademiasDto
{
    public class AcademiaResponseDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? LogoUrl { get; set; }
        public string? Cidade { get; set; }
        public string? RedeSocial { get; set; }
        public string? Responsavel { get; set; }
        public string? MercadoPagoPublicKey { get; set; }
        public int totalAlunos { get; set; }
        public int totalProfessores  { get; set; }
        public bool Ativo { get; set; }
    }
}
