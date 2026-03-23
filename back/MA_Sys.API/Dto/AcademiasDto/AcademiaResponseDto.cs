using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.AcademiasDto
{
    public class AcademiaResponseDto
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
    }
}