using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class Federacao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        public Users? OwnerUser { get; set; }

         public string? MercadoPagoPublicKey { get; set; }
        public string? MercadoPagoAccessToken { get; set; }
    }
}
