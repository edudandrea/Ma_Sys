using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Modalidade
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string? NomeModalidade { get; set; }
        public bool Ativo { get; set; }
        public int TotalAlunos { get; set; }
        public int TotalProf { get; set; }
    }
}