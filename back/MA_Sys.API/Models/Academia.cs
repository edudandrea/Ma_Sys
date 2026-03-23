using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MA_SYS.Api.Models
{
    public class Academia
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Email { get; set; }
        public string? RedeSocial { get; set; }
        public string? Responsavel { get; set; }
        public string? Telefone { get; set; }
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }
    }
}