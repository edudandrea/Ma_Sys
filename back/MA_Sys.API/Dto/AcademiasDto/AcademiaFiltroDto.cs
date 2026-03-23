using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MA_Sys.API.Dto.AcademiasDto
{
    public class AcademiaFiltroDto
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public int Telefone { get; set; }
    }
}