using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Aula
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AcademiaId { get; set; }

        public int ProfessorId { get; set; }

        public string? Modalidade { get; set; }

        public DateTime Horario { get; set; }
    }
}