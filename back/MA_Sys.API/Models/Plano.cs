using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Plano
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? AcademiaId { get; set; }
        public int? OwnerUserId { get; set; }
        public string? Nome { get; set; }
        public decimal Valor { get; set; }
        public int DuracaoMeses { get; set; }
        public bool Ativo { get; set; }
        public int TotalAlunos { get; set; }
        public Academia? Academia { get; set; }
        public Users? OwnerUser { get; set; }
    }
}
