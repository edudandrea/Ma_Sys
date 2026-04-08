using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.Planos
{
    public class PlanosDto
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string? Nome { get; set; }
        public decimal Valor { get; set; }
        public int DuracaoMeses { get; set; }
        public bool Ativo { get; set; }
    }
}