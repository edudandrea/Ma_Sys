using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MA_SYS.Api.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "O campo Login é obrigatório")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "O login deve ter entre 4 e 20 caracteres")]
        public string? Login { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage = "O campo senha é obrigatório")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 10 caracteres")]
        public string? Password { get; set; }
        public string? Role { get; set; }

        [Required(ErrorMessage = "O campo função é obrigatório")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "A função deve ter entre 4 e 20 caracteres")]
        public string? Function { get; set; }
        public int AcademiaId { get; set; }
        public Academia? Academia { get; set; }
    }
}