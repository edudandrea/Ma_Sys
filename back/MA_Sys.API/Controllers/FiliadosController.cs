using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Filiados;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace MA_Sys.API.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FiliadosController : BaseController
    {
        private readonly FiliadosService _service;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public FiliadosController(FiliadosService service, IUserRepository userRepository, IConfiguration configuration)
        {
            _service = service;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult List()
        {
            var (role, academiaId, userId) = GetUserInfo();
            var filiados = _service.List(role, academiaId, userId);
            return Ok(filiados);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            var academia = _service.GetById(id, role, academiaId, userId);

            if (academia == null)
                return NotFound();

            return Ok(academia);
        }        

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add([FromBody] FiliadosCreateDto dto)
        {
            if (_userRepository.HasAnyUser() &&
                !RoleScope.IsAdmin(GetUserRole()) &&
                !RoleScope.IsSuperAdmin(GetUserRole()) &&
                !RoleScope.IsFederacao(GetUserRole()))
            {
                return Forbid();
            }

            _service.Add(dto, GetUserRole(), GetUserId());

            return Ok(new
            {
                sucesso = true,
                mensagem = "Fiiado cadastrado com sucesso"
            });
        }

        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            var role = GetUserRole();
            if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role) && !RoleScope.IsFederacao(role))
            {
                return Forbid();
            }

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Selecione uma imagem para upload." });

            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
            var extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!extensoesPermitidas.Contains(extensao))
                return BadRequest(new { message = "Formato de imagem invalido." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "A imagem deve ter no maximo 5MB." });

            var uploadsPath = GetAcademiaUploadsPath();
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid():N}{extensao}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new
            {
                logoUrl = $"/api/Filiados/logo/{fileName}"
            });
        }

        [AllowAnonymous]
        [HttpGet("logo/{fileName}")]
        public IActionResult GetLogo(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
                return BadRequest(new { message = "Arquivo da logo invalido." });

            var filePath = Path.Combine(GetAcademiaUploadsPath(), safeFileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType);
        }

        private string GetAcademiaUploadsPath()
        {
            var uploadsRoot = _configuration["UPLOADS_ROOT"];
            if (!string.IsNullOrWhiteSpace(uploadsRoot))
            {
                return Path.Combine(uploadsRoot, "filiados");
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "filiados");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] FiliadosUpdateDto dto, int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Update(id, dto, role, academiaId, userId);

            return Ok();
        }

        [HttpPatch("{id}/status")]
        public IActionResult AtualizarStatus(int id, [FromBody] bool ativo)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.UpdateStatus(id, role, academiaId, userId, ativo);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var (role, academiaId, userId) = GetUserInfo();
            _service.Delete(id, role, academiaId, userId);

            return NoContent();
        }
    }
}
