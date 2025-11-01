using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/relatorios")]
    public class RelatorioMobileController : BaseController
    {
        private readonly IJwtToken _jwtToken;
        private readonly RelatorioService _relatorioService;

        public RelatorioMobileController(
            INotificador notificador,
            IJwtToken jwtToken,
            RelatorioService relatorioService) : base(notificador)
        {
            _jwtToken = jwtToken;
            _relatorioService = relatorioService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListarRelatorios(
            [FromQuery] string? fazenda,
            [FromQuery] string? talhao,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);

            // Validar parâmetros de data
            if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
            {
                return BadRequest(new { message = "Data inicial não pode ser maior que data final." });
            }

            // Validar paginação
            if (page < 1)
            {
                return BadRequest(new { message = "Número da página deve ser maior ou igual a 1." });
            }

            if (limit < 1 || limit > 100)
            {
                return BadRequest(new { message = "Limite deve estar entre 1 e 100." });
            }

            // Criar query
            var query = new QueryRelatorioMobile
            {
                Fazenda = fazenda,
                Talhao = talhao,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Page = page,
                Limit = limit
            };

            // Buscar relatórios
            var resultado = await _relatorioService.ListarRelatoriosMobileAsync(userId, query);

            return Ok(resultado);
        }
    }
}

