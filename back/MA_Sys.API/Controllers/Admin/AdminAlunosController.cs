using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MA_Sys.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminAlunosController : ControllerBase
    {
        private readonly IAlunoRepository _repo;

        public AdminAlunosController(IAlunoRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var academias = _repo
            .Query().Select(a => new
            {
                a.Id,
                a.Nome,
                a.Email,
                a.Telefone,
                a.AcademiaId
            }).ToList();

            return Ok(academias);
        }

    }
}