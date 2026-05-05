using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class Filiados
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public Users? OwnerUser { get; set; }
    }
}