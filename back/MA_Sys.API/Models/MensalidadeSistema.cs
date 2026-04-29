using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class MensalidadeSistema
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public int PrazoPagamentoDias { get; set; }
        public int MesesUso { get; set; }
        public bool Ativo { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataCadastro { get; set; }
        public int? OwnerUserId { get; set; }
        public Users? OwnerUser { get; set; }
    }
}
