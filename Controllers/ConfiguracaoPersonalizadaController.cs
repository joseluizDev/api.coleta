using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/configuracao-personalizada")]
    public class ConfiguracaoPersonalizadaController : BaseController
    {
        private readonly ConfiguracaoPersonalizadaService _configuracaoPersonalizadaService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public ConfiguracaoPersonalizadaController(
            ConfiguracaoPersonalizadaService configuracaoPersonalizadaService,
            INotificador notificador,
            IJwtToken jwtToken) : base(notificador)
        {
            _configuracaoPersonalizadaService = configuracaoPersonalizadaService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet]
        [Authorize]
        public IActionResult ListarConfiguracoesPersonalizadas()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var configuracoes = _configuracaoPersonalizadaService.ListarConfiguracoesPersonalizadas(userId);
            return Ok(configuracoes);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult BuscarConfiguracaoPersonalizada(Guid id)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var configuracao = _configuracaoPersonalizadaService.BuscarConfiguracaoPersonalizadaPorId(id, userId);

            if (configuracao == null)
            {
                return NotFound(new { message = "Configuração personalizada não encontrada." });
            }

            return Ok(configuracao);
        }

        [HttpPost]
        [Authorize]
        public IActionResult SalvarConfiguracaoPersonalizada([FromBody] ConfiguracaoPersonalizadaRequestDTO configuracao)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var configuracaoSalva = _configuracaoPersonalizadaService.SalvarConfiguracaoPersonalizada(configuracao, userId);

            return CreatedAtAction(nameof(BuscarConfiguracaoPersonalizada), new { id = configuracaoSalva.Id }, configuracaoSalva);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult AtualizarConfiguracaoPersonalizada(Guid id, [FromBody] ConfiguracaoPersonalizadaRequestDTO configuracao)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var configuracaoAtualizada = _configuracaoPersonalizadaService.AtualizarConfiguracaoPersonalizada(id, configuracao, userId);

            if (configuracaoAtualizada == null)
            {
                return NotFound(new { message = "Configuração personalizada não encontrada." });
            }

            return Ok(configuracaoAtualizada);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeletarConfiguracaoPersonalizada(Guid id)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var resultado = _configuracaoPersonalizadaService.DeletarConfiguracaoPersonalizada(id, userId);

            if (!resultado)
            {
                return NotFound(new { message = "Configuração personalizada não encontrada." });
            }

            return NoContent();
        }
    }
}
