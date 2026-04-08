using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers
{
    [Route("[controller]")]
    public class BaseController : Controller
    {
       protected string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        protected int? GetAcademiaId()
        {
            var claim = User.FindFirst("AcademiaId");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            
                return null;
            if(int.TryParse(claim.Value, out int academiaId))
            
                return academiaId;            
            
            return null;
        }

        protected (string role, int? academiaId) GetUserInfo()
        {
            return (GetUserRole(), GetAcademiaId());
        }
    }
}