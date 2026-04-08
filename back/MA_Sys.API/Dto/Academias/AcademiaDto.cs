namespace MA_SYS.Api.Dto
{
    public class AcademiaDto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Email { get; set; }
        public string? RedeSocial { get; set; }
        public DateTime DataCadastro { get; set; }
        public string? Responsavel { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
    }
}