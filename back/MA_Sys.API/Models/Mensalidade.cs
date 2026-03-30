namespace MA_SYS.Api.Models
{
    public class Mensalidade
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public int AlunoId { get; set; }
        public decimal Valor { get; set; }
        public DateTime Vencimento { get; set; }
        public bool Pago { get; set; }
    }
}