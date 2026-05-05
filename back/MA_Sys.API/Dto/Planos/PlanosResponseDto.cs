using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.Planos
{
    public class PlanosResponseDto
    {
        public int Id { get; set; }
        public int? AcademiaId { get; set; }
        public int? OwnerUserId { get; set; }
        public string? AcademiaNome { get; set; }
        public string? Escopo { get; set; }
        public string? Nome { get; set; }
        public decimal Valor { get; set; }
        public int DuracaoMeses { get; set; }
        public bool Ativo { get; set; }
        public int TotalAlunos { get; set; }
        public Academia? Academia { get; set; }

    }
}
