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
    public class AdminAcademiasController : Controller
    {
        private readonly IAcademiaRepository _repo;

        public AdminAcademiasController(IAcademiaRepository repo)
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
                a.Ativo
            }).ToList();

            return Ok(academias);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody]bool ativo)
        {
            var academia = _repo.Query().FirstOrDefault(a => a.Id == id);

            if (academia == null)
            return NotFound();

            academia.Ativo = ativo;

            _repo.Update(academia);
            _repo.Save();

            return Ok();
        }
    }
}