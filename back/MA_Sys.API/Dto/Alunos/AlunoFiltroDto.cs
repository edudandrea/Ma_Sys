using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.Alunos
{
    public class AlunoFiltroDto
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public string? CPF { get; set; }
    }
}