using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/[controller]")]

    public class AdminDashController : ControllerBase
    {
        private readonly AdminService _service;

        public AdminDashController(AdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                totalAcademias = _service.Totalacademia(),
                totalAlunos = _service.TotalAlunos()
            });
        }
    }   
    
}