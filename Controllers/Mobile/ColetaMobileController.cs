using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/coleta")]
    public class ColetaMobileController : BaseController
    {

        private readonly IJwtToken _jwtToken;
        private readonly VisualizarMapaService _visualizarMapaService;
        public ColetaMobileController(INotificador notificador, IJwtToken jwtToken, VisualizarMapaService visualizarMapaService) : base(notificador)
        {
            _jwtToken = jwtToken;
            _visualizarMapaService = visualizarMapaService;
        }

        [HttpGet]
        [Route("listar")]
        [Authorize]
        public IActionResult Listar()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var safras = _visualizarMapaService.ListarMobile(userId);
            return Ok(safras);
        }

        [HttpGet]
        [Route("listar-por-fazenda")]
        [Authorize]
        public IActionResult ListarPorFazenda()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var coletasPorFazenda = _visualizarMapaService.ListarMobilePorFazenda(userId);
            return Ok(coletasPorFazenda);
        }

        [HttpPost]
        [Route("salva")]
        [Authorize]
        public IActionResult SalvarColeta([FromBody] ColetaMobileDTO coleta)
        {
            var token = ObterIDDoToken();
            if (token != null)
            {
                Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);

                bool? resultado = _visualizarMapaService.SalvarColeta(userId, coleta);
                if (resultado != false)
                {
                    return Ok(resultado);
                }
                return BadRequest(resultado);
            }
            else
            {
                return BadRequest(new { message = "Token inválido." });

            }
        }
    }
}
