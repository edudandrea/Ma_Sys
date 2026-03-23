using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MA_Sys.API.Dto.AcademiasDto
{
    public class AcademiaCreateDto
    {
        public string? Nome { get; set; }
        public string? Cidade { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
    }
}