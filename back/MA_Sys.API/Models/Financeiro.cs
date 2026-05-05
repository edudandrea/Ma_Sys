using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class Financeiro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? AcademiaId { get; set; }
        public int? OwnerUserId { get; set; }
        public int? AlunoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? FormaPagamentoId { get; set; }
        public FormaPagamento? FormaPagamento { get; set; }
        public Academia? Academia { get; set; }
        public Users? OwnerUser { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Origem { get; set; } = "Manual";
        public bool Pago { get; set; }
    }
}
