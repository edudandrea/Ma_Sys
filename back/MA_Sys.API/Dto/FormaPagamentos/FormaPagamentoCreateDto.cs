using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.FormaPagamentos
{
    public class FormaPagamentoCreateDto
    {
        public int? AcademiaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public decimal Taxa { get; set; }
        public int Parcelas { get; set; }
        public int Dias { get; set; }
    }
}
