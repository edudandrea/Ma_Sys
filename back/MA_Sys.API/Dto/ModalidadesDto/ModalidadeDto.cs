using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_SYS.Api.Dto
{
    public class ModalidadeDto
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string? NomeModalidade { get; set; }
        public bool Ativo { get; set; }
        public int TotalAlunos { get; set; }
    }
}