using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class TurmaAluno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TurmaId { get; set; }
        public int AlunoId { get; set; }
        public Turma? Turma { get; set; }
        public Aluno? Aluno { get; set; }
    }
}
