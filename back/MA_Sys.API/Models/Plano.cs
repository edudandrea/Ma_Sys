namespace MA_SYS.Api.Models
{
    public class Plano
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string? Nome { get; set; }
        public decimal Valor { get; set; }
        public int DuracaoMeses { get; set; }
    }
}