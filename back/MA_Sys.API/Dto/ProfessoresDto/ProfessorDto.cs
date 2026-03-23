using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_SYS.Api.Dto
{
    public class ProfessorDto
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string? Nome { get; set; }
        public string? Graduacao { get; set; }

        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public int ModalidadeId { get; set; }
        public bool Ativo { get; set; }
    }
}