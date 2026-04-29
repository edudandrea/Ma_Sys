using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.Matriculas
{
    public class MatriculasUpdateDto
    {
               

        public int AlunoId { get; set; }

        public int PlanoId { get; set; }

        public int FormaPagamentoId { get; set; }
        public DateTime DataInicio { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
