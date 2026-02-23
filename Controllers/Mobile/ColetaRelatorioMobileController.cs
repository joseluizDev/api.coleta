using api.cliente.Interfaces;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/coleta-relatorio")]
    public class ColetaRelatorioMobileController : BaseController
    {
        private readonly VisualizarMapaService _visualizarMapaService;
        private readonly RelatorioService _relatorioService;
        private readonly ConfiguracaoPersonalizadaService _configuracaoPersonalizadaService;
        private readonly ConfiguracaoPadraoService _configuracaoPadraoService;
        private readonly IJwtToken _jwtToken;

        public ColetaRelatorioMobileController(
            VisualizarMapaService visualizarMapaService,
            RelatorioService relatorioService,
            ConfiguracaoPersonalizadaService configuracaoPersonalizadaService,
            ConfiguracaoPadraoService configuracaoPadraoService,
            INotificador notificador,
            IJwtToken jwtToken)
            : base(notificador)
        {
            _visualizarMapaService = visualizarMapaService;
            _relatorioService = relatorioService;
            _configuracaoPersonalizadaService = configuracaoPersonalizadaService;
            _configuracaoPadraoService = configuracaoPadraoService;
            _jwtToken = jwtToken;
        }



   
        [HttpGet]
        [Route("relatorios")]
        [Authorize]
        public async Task<IActionResult> ListarRelatorios()
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);

            if (!userIdNullable.HasValue)
            {
                return BadRequest(new { message = "Token inválido ou usuário não encontrado." });
            }

            var relatorios = await _relatorioService.ListarRelatoriosPorUploadAsync(userIdNullable.Value, new api.coleta.models.dtos.QueryRelatorio());
            return Ok(relatorios);
        }

       
        [HttpGet]
        [Route("relatorio/{relatorioId:guid}")]
        [Authorize]
        public async Task<IActionResult> ObterRelatorioCompleto(Guid relatorioId)
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);

            if (!userIdNullable.HasValue)
            {
                return BadRequest(new { message = "Token inválido ou usuário não encontrado." });
            }

            var relatorio = await _relatorioService.GetRelatorioCompletoAsync(relatorioId, userIdNullable.Value);
            
            if (relatorio == null)
            {
                return NotFound(new { message = "Relatório não encontrado." });
            }

            return Ok(relatorio);
        }

    }
}
