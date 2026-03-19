using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Models
{
    public class UserResponse
    {
        public Users? Users { get; set; }
        public string? Token { get; set; }
    }
}