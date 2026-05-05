using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.Matriculas
{
    public class MatriculasResponseDto
    {
        public int Id { get; set; }

        public int AcademiaId { get; set; }

        public int AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public int PlanoId { get; set; }
        public string? PlanoNome { get; set; }
        public decimal? PlanoValor { get; set; }
        public int FormaPagamentoId { get; set; }
        public string? FormaPagamentoNome { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? MensalidadeStatus { get; set; }
        public DateTime? DataVencimentoMensalidade { get; set; }
        public int? DiasParaVencimento { get; set; }
    }
}
