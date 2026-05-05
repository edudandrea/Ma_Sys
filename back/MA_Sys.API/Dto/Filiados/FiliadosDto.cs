using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Dto.Federacao
{
    public class FiliadosDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Email { get; set; }
        public string? RedeSocial { get; set; }
        public string? Responsavel { get; set; }
        public string? Telefone { get; set; }
        public string? LogoUrl { get; set; }
        public int? OwnerUserId { get; set; }
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
        
    }
}