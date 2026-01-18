using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    /// <summary>
    /// Controller de compatibilidade para listagem de planos.
    ///
    /// NOTA: Os planos são gerenciados pelo Gateway de Pagamentos Python (PostgreSQL).
    /// Este controller apenas faz proxy para o gateway para manter compatibilidade
    /// com o frontend existente que chama /api/plano/listar.
    ///
    /// Gateway Endpoint: GET /api/v1/planos
    /// </summary>
    [ApiController]
    [Route("api/plano")]
    public class PlanoController : ControllerBase
    {
        private readonly IGatewayService _gatewayService;
        private readonly ILogger<PlanoController> _logger;

        public PlanoController(
            IGatewayService gatewayService,
            ILogger<PlanoController> logger)
        {
            _gatewayService = gatewayService;
            _logger = logger;
        }

        /// <summary>
        /// Lista planos disponiveis via Gateway de Pagamentos.
        /// Endpoint de compatibilidade para frontend.
        /// </summary>
        [HttpGet("listar")]
        [AllowAnonymous]
        public async Task<IActionResult> Listar()
        {
            _logger.LogDebug("Listando planos via gateway");
            var planos = await _gatewayService.ListarPlanosAsync();
            return Ok(planos);
        }

        /// <summary>
        /// Obtem plano por ID via Gateway de Pagamentos.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorId([FromRoute] Guid id)
        {
            var planos = await _gatewayService.ListarPlanosAsync();
            var plano = planos.FirstOrDefault(p => p.Id == id);

            if (plano == null)
            {
                return NotFound(new { message = "Plano não encontrado" });
            }

            return Ok(plano);
        }
    }
}
