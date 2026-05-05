using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.Filiados
{
    public class FiliadosCreateDto
    {
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? LogoUrl { get; set; }
        public string? RedeSocial { get; set; }
        public string? Responsavel { get; set; }
        public int? OwnerUserId { get; set; }
    }
}
