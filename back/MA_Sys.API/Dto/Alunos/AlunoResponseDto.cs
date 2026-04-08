namespace MA_Sys.API.Dto.Alunos
{
    public class AlunoResponseDto
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int PlanoId { get; set; }
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Endereco { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? CEP { get; set; }
        public string? Email { get; set; }
        public string? RedeSocial { get; set; }
        public int ModalidadeId { get; set; }
        public DateTime DataNascimento { get; set; }
        public DateTime DataCadastro { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
        public string? Graduacao { get; set; }
        public string? Obs { get; set; }
    }
}