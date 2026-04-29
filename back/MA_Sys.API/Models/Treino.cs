using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Treino
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Objetivo { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; } = true;
        public ICollection<TreinoExercicio> Exercicios { get; set; } = [];
    }
}
