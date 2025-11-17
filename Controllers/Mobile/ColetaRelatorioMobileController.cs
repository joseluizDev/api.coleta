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

        /// <summary>
        /// Visualizar coleta por ID
        /// </summary>
        /// <param name="id">ID da coleta</param>
        /// <returns>Detalhes da coleta</returns>
        [HttpGet]
        [Route("coleta/{id}")]
        [Authorize]
        public IActionResult VisualizarColetaPorId([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            
            if (!userIdNullable.HasValue)
            {
                return BadRequest(new { message = "Token inválido ou usuário não encontrado." });
            }

            var coleta = _visualizarMapaService.BuscarVisualizarMapaPorId(userIdNullable.Value, id);
            
            if (coleta == null)
            {
                return NotFound(new { message = "Coleta não encontrada." });
            }

            return Ok(coleta);
        }

    
        [HttpGet]
        [Route("relatorio/{coletaId}")]
        [Authorize]
        public async Task<IActionResult> BuscarRelatorioPorColetaId([FromRoute] Guid coletaId)
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            
            if (!userIdNullable.HasValue)
            {
                return BadRequest(new { message = "Token inválido ou usuário não encontrado." });
            }

            var relatorio = await _relatorioService.GetRelario(coletaId, userIdNullable.Value);
            
            if (relatorio == null)
            {
                return NotFound(new { message = "Relatório não encontrado para esta coleta." });
            }

            return Ok(relatorio);
        }

        /// <summary>
        /// Buscar configurações personalizadas com fallback para configurações padrão
        /// </summary>
        /// <returns>Lista de configurações personalizadas ou padrão</returns>
        [HttpGet]
        [Route("configuracoes/fallback")]
        [Authorize]
        public IActionResult BuscarConfiguracoesComFallback()
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            
            if (!userIdNullable.HasValue)
            {
                return BadRequest(new { message = "Token inválido ou usuário não encontrado." });
            }

            // Buscar configurações personalizadas do usuário
            var configuracoesPersonalizadas = _configuracaoPersonalizadaService.ListarConfiguracoesPersonalizadas(userIdNullable.Value);
            
            // Se existem configurações personalizadas, retorná-las
            if (configuracoesPersonalizadas != null && configuracoesPersonalizadas.Count > 0)
            {
                return Ok(new 
                { 
                    tipo = "personalizada",
                    configuracoes = configuracoesPersonalizadas 
                });
            }
            
            // Caso contrário, retornar configurações padrão
            var configuracoesPadrao = _configuracaoPadraoService.ListarConfiguracoes();
            return Ok(new 
            { 
                tipo = "padrao",
                configuracoes = configuracoesPadrao 
            });
        }
    }
}
