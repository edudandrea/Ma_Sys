namespace MA_SYS.Api.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        public int AcademiaId { get; set; }

        public int AlunoId { get; set; }

        public int PlanoId { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }
    }
}