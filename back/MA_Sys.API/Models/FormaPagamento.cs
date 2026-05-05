using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class FormaPagamento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? AcademiaId { get; set; }
        public int? OwnerUserId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public decimal Taxa { get; set; }
        public int Parcelas { get; set; }
        public int Dias { get; set; }
        public Academia? Academia { get; set; }
        public Users? OwnerUser { get; set; }
    
    }
}
