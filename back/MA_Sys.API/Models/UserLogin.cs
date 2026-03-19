using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "O campo Login é obrigatório")]
        [StringLength(10, MinimumLength =4, ErrorMessage = "O login deve ter entre 4 e 20 caracteres")]

        public string? Login { get; set; }
        
        [Required(ErrorMessage = "O campo senha é obrigatório")]
        [StringLength(10, MinimumLength =6, ErrorMessage = "A senha deve ter entre 6 e 10 caracteres")]
        public string? Password { get; set; }
    }
}