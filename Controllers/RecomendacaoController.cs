using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de recomendações
    /// </summary>
    [ApiController]
    [Route("api/recomendacao")]
    [Authorize]
    public class RecomendacaoController : BaseController
    {
        private readonly RecomendacaoService _recomendacaoService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public RecomendacaoController(IJwtToken jwtToken, INotificador notificador, RecomendacaoService recomendacaoService) : base(notificador)
        {
            _jwtToken = jwtToken;
            _notificador = notificador;
            _recomendacaoService = recomendacaoService;
        }

        /// <summary>
        /// Criar uma nova recomendação para um relatório
        /// </summary>
        /// <param name="dto">Dados da recomendação</param>
        /// <returns>Recomendação criada</returns>
        /// <response code="200">Recomendação criada com sucesso</response>
        /// <response code="400">Requisição inválida</response>
        /// <response code="401">Não autorizado</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Criar([FromBody] RecomendacaoDTO dto)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var recomendacao = await _recomendacaoService.CriarRecomendacaoAsync(dto, userId);
            
            if (recomendacao == null)
            {
                return BadRequest(new { message = "Erro ao criar recomendação." });
            }

            return Ok(recomendacao);
        }

        /// <summary>
        /// Buscar recomendações de um relatório específico
        /// </summary>
        /// <param name="relatorioId">ID do relatório</param>
        /// <returns>Lista de recomendações</returns>
        /// <response code="200">Recomendações encontradas</response>
        /// <response code="401">Não autorizado</response>
        [HttpGet("relatorio/{relatorioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BuscarPorRelatorio([FromRoute] Guid relatorioId)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var recomendacoes = await _recomendacaoService.BuscarPorRelatorioAsync(relatorioId, userId);
            return Ok(recomendacoes);
        }

        /// <summary>
        /// Atualizar uma recomendação existente
        /// </summary>
        /// <param name="id">ID da recomendação</param>
        /// <param name="dto">Dados atualizados</param>
        /// <returns>Recomendação atualizada</returns>
        /// <response code="200">Recomendação atualizada com sucesso</response>
        /// <response code="400">Requisição inválida</response>
        /// <response code="404">Recomendação não encontrada</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar([FromRoute] Guid id, [FromBody] RecomendacaoDTO dto)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var atualizada = await _recomendacaoService.AtualizarRecomendacaoAsync(id, dto, userId);
            
            if (atualizada == null)
            {
                return NotFound(new { message = "Recomendação não encontrada." });
            }

            return Ok(atualizada);
        }

        /// <summary>
        /// Deletar uma recomendação
        /// </summary>
        /// <param name="id">ID da recomendação</param>
        /// <returns>Confirmação de exclusão</returns>
        /// <response code="200">Recomendação deletada com sucesso</response>
        /// <response code="404">Recomendação não encontrada</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deletar([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var deletada = await _recomendacaoService.DeletarRecomendacaoAsync(id, userId);
            
            if (!deletada)
            {
                return NotFound(new { message = "Recomendação não encontrada." });
            }

            return Ok(new { message = "Recomendação deletada com sucesso." });
        }
    }
}
