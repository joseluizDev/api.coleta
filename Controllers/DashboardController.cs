using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.dashboard.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public DashboardController(DashboardService dashboardService, IJwtToken jwtToken, INotificador notificador)
            : base(notificador)
        {
            _dashboardService = dashboardService;
            _jwtToken = jwtToken;
            _notificador = notificador;
        }

        [HttpGet("resumo")]
        [Authorize]
        public IActionResult ObterResumo()
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != Guid.Empty)
            {
                var resumo = _dashboardService.ObterResumo(userId);
                return Ok(resumo);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
