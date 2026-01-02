using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/license")]
    [Authorize]
    public class LicenseController : BaseController
    {
        private readonly LicenseService _licenseService;

        public LicenseController(
            INotificador notificador,
            LicenseService licenseService) : base(notificador)
        {
            _licenseService = licenseService;
        }

        /// <summary>
        /// Obtém o status completo da licença de um cliente
        /// </summary>
        [HttpGet("status/{clienteId}")]
        public async Task<IActionResult> ObterStatusLicenca(Guid clienteId)
        {
            var status = await _licenseService.VerificarLicencaDoClienteAsync(clienteId);
            return CustomResponse(status);
        }

        /// <summary>
        /// Verifica se um cliente pode adicionar mais hectares
        /// </summary>
        [HttpGet("verificar-limite/{clienteId}")]
        public async Task<IActionResult> VerificarLimiteHectares(Guid clienteId, [FromQuery] decimal hectaresAdicionais)
        {
            var podeAdicionar = await _licenseService.ValidarLimiteHectaresAsync(clienteId, hectaresAdicionais);
            return CustomResponse(new { podeAdicionar, hectaresAdicionais });
        }

        /// <summary>
        /// Valida se a licença de um cliente está válida (ativa e não expirada)
        /// </summary>
        [HttpGet("validar/{clienteId}")]
        public async Task<IActionResult> ValidarLicenca(Guid clienteId)
        {
            var resultado = await _licenseService.ValidarLicencaAsync(clienteId);
            return CustomResponse(resultado);
        }
    }
}
