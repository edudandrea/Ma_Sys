using MA_Sys.API.Controllers;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.AcademiasDto;
using MA_Sys.API.Security;
using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace MA_SYS.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AcademiasController : BaseController
    {
        private readonly AcademiaService _service;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AcademiasController(AcademiaService service, IUserRepository userRepository, IConfiguration configuration)
        {
            _service = service;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult List()
        {
            var (role, academiaId, userId) = GetUserInfo();
            var academias = _service.List(role, academiaId, userId);
            return Ok(academias);
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

        [AllowAnonymous]
        [HttpGet("public/{slug}/pagamento-config")]
        public IActionResult GetPagamentoConfigPublico(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest(new { message = "Slug da academia nao informado." });

            var config = _service.List("SuperAdmin", null, null)
                .Where(a => string.Equals(a.Slug, slug, StringComparison.OrdinalIgnoreCase))
                .Select(a => new AcademiaPagamentoConfigDto
                {
                    ChavePix = a.ChavePix,
                    MercadoPagoPublicKey = a.MercadoPagoPublicKey
                })
                .FirstOrDefault();

            if (config == null)
                return NotFound(new { message = "Academia nao encontrada." });

            return Ok(config);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Add([FromBody] AcademiaCreateDto dto)
        {
            if (_userRepository.HasAnyUser() &&
                !RoleScope.IsAdmin(GetUserRole()) &&
                !RoleScope.IsSuperAdmin(GetUserRole()))
            {
                return Forbid();
            }

            _service.Add(dto, GetUserRole(), GetUserId());

            return Ok(new
            {
                sucesso = true,
                mensagem = "Academia cadastrada com sucesso"
            });
        }

        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            var role = GetUserRole();
            if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role))
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
                logoUrl = $"/api/Academias/logo/{fileName}"
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
                return Path.Combine(uploadsRoot, "academias");
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "academias");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] AcademiaUpdateDto dto, int id)
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
