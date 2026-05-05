using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_Sys.API.Models
{
    public class PagamentoFiliado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FiliadoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = "Pendente";
        public string? Descricao { get; set; }
        public string? ExternalId { get; set; }
        public string? FormaPagamentoNome { get; set; }
        public Filiados? Filiado { get; set; }
    }
}
