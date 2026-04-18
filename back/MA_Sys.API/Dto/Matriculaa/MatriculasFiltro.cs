using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.Matriculas
{
    public class MatriculasFiltro
    {
        public int Id { get; set; }

        public int AcademiaId { get; set; }

        public int AlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;

        public int PlanoId { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }
        public string Status { get; set; } = string.Empty;
        
    }
}