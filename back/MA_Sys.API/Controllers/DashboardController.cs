using MA_Sys.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MA_Sys.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var (role, academiaId, userId) = GetUserInfo();
            var dashboard = _service.GetDashboard(role, academiaId, userId);
            return Ok(dashboard);
        }

        [HttpGet("federacao")]
        public IActionResult GetFederacao()
        {
            try
            {
                var dashboard = _service.GetDashboardFederacao(GetUserRole(), GetUserId());
                return Ok(dashboard);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
