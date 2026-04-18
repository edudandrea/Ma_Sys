using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Dto.Matriculas
{
    public class MatriculasCreateDto
    {
        public int AcademiaId { get; set; }

        public int AlunoId { get; set; }

        public int PlanoId { get; set; }

        public DateTime? DataInicio { get; set; }

        public int FormaPgtoId { get; set; }
        
    }
}