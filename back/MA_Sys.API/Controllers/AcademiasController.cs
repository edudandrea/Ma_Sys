using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MA_Sys.API.Dto.AcademiasDto;
using MA_Sys.API.Services;
using MA_SYS.Api.Data;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcademiasController : Controller
    {
        private readonly AcademiaService _service;

        public AcademiasController(AcademiaService service)
        {
            _service = service;
        }

        /// <summary>
        /// Busca academias utilizando filtros opcionais. Informe ao menos um filtro para realizar a busca.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nomeAcademia"></param>
        /// <param name="cidade"></param>
        /// <param name="ativo"></param>
        /// <returns></returns>

        

        /// <summary>
        /// Adiciona uma nova academia. Campos obrigatórios: NomeAcademia, Cidade.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddAcademia([FromBody] AcademiaCreateDto dto)
        {
            _service.Criar(dto);

            return Ok("Academia criada om sucesso");
        }

        
               

        /// <summary>
        /// Atualiza uma academia existente. Campos obrigatórios: NomeAcademia, Cidade, Email.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        

        

    }
}

