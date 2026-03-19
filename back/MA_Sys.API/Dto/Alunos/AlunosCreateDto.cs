using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace MA_Sys.API.Dto.Alunos
{
    public class AlunosCreateDto
    {
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public int ModalidadeId { get; set; }
        public string? Graduacao { get; set; }
        public DateTime DataNescimento { get; set; }
    }
}