namespace MA_Sys.API.Dto.Alunos
{
    public class AlunoUpdateDto
    {
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Endereco { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? RedeSocial { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public int ModalidadeId { get; set; }
        public string? Graduacao { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}