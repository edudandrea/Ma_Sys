using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Exercicio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? GrupoMuscular { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
