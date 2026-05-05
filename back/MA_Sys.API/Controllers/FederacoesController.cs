using MA_Sys.API.Dto.Federacoes;
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
    public class FederacoesController : BaseController
    {
        private readonly FederacaoService _service;
        private readonly IConfiguration _configuration;

        public FederacoesController(FederacaoService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult List()
        {
            return Ok(_service.List(GetUserRole(), GetUserId(), GetFederacaoId()));
        }

        [HttpPost]
        public IActionResult Add([FromBody] FederacaoCreateUpdateDto dto)
        {
            _service.Add(dto, GetUserRole(), GetUserId());
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] FederacaoCreateUpdateDto dto)
        {
            _service.Update(id, dto, GetUserRole(), GetUserId(), GetFederacaoId());
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] bool ativo)
        {
            _service.UpdateStatus(id, ativo, GetUserRole(), GetUserId());
            return NoContent();
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

            var uploadsPath = GetUploadsPath();
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid():N}{extensao}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new { logoUrl = $"/api/Federacoes/logo/{fileName}" });
        }

        [AllowAnonymous]
        [HttpGet("logo/{fileName}")]
        public IActionResult GetLogo(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
                return BadRequest(new { message = "Arquivo da logo invalido." });

            var filePath = Path.Combine(GetUploadsPath(), safeFileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType);
        }

        private string GetUploadsPath()
        {
            var uploadsRoot = _configuration["UPLOADS_ROOT"];
            return !string.IsNullOrWhiteSpace(uploadsRoot)
                ? Path.Combine(uploadsRoot, "federacoes")
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "federacoes");
        }
    }
}
