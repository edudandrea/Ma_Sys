using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class PagamentoAcademia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = "Pendente";
        public string? Descricao { get; set; }
        public string? ExternalId { get; set; }
        public string? FormaPagamentoNome { get; set; }
        public int? MensalidadeSistemaId { get; set; }
        public Academia? Academia { get; set; }
        public MensalidadeSistema? MensalidadeSistema { get; set; }
    }
}
