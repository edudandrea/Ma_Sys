using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Turma
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string DiasSemana { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public ICollection<TurmaAluno> Alunos { get; set; } = [];
    }
}
