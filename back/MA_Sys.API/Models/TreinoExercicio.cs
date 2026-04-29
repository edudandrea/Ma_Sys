using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class TreinoExercicio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TreinoId { get; set; }
        public int ExercicioId { get; set; }
        public int Ordem { get; set; }
        public int Series { get; set; }
        public int Repeticoes { get; set; }
        public string? Descanso { get; set; }
        public string? Observacoes { get; set; }
        public Treino? Treino { get; set; }
        public Exercicio? Exercicio { get; set; }
    }
}
