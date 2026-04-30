using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Academia
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Email { get; set; }
        public string? RedeSocial { get; set; }
        public string? Responsavel { get; set; }
        public string? Telefone { get; set; }
        public string? LogoUrl { get; set; }
        public string? MercadoPagoPublicKey { get; set; }
        public string? MercadoPagoAccessToken { get; set; }
        public int? OwnerUserId { get; set; }
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public ICollection<Modalidade>? Alunos { get; set; }
        public ICollection<Professor>? Professores { get; set; }
        public Users? OwnerUser { get; set; }
    }
}
