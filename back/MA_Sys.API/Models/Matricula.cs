using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.ConstrainedExecution;

namespace MA_SYS.Api.Models
{
    public class Matricula
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int AlunoId { get; set; }
        public int PlanoId { get; set; }
        public int FormaPagamentoId { get; set; }
        public DateTime DataInicio { get; set; }
        public bool MensalidadePaga { get; set; }
        public DateTime? DataPagamento { get; set; }
        public DateTime DataFim { get; set; }
        public string Status { get; set; } = string.Empty;
        public Aluno Aluno { get; set; } = null!;
        public Plano Plano { get; set; } = null!;
    }
}