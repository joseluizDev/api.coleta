using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.models.dtos;
using api.coleta.Services;
using api.safra.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/relatorio")]
    public class RelatorioController : BaseController
    {
        private readonly RelatorioService _relatorioService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public RelatorioController(IJwtToken jwtToken, INotificador notificador, RelatorioService relatorioService) : base(notificador)
        {
            _jwtToken = jwtToken;
            _notificador = notificador;
            _relatorioService = relatorioService;
        }


        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadData([FromForm] RelatorioDTO relatorio)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                string url = await _relatorioService.SalvarRelatorio(relatorio, userId);
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest(new { message = "Erro ao salvar o relatório." });
                }
                return Ok(new { message = "Upload realizado com sucesso!" });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetRelatorio([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                // Usar GetRelatorioCompletoAsync para incluir classificações com configurações personalizadas
                var relatorio = await _relatorioService.GetRelatorioCompletoAsync(id, userId);

                return Ok(relatorio);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("buscar")]
        public async Task<IActionResult> ListarRelatoriosPorUpload([FromQuery] QueryRelatorio query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var relatorios = await _relatorioService.ListarRelatoriosPorUploadAsync(userId, query);
                return Ok(relatorios);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("atualizar/jsonRelatorio")]
        public async Task<IActionResult> AtualizarJsonRelatorio([FromBody] AtualizarJsonRelatorioDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Requisição inválida." });
            }

            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var atualizado = await _relatorioService.AtualizarJsonRelatorioAsync(request.ColetaId, request.RelatorioId, userId, request.JsonRelatorio);
                if (atualizado)
                {
                    return Ok(new { message = "Relatório atualizado com sucesso." });
                }

                return NotFound(new { message = "Relatório não encontrado para a coleta informada." });
            }

            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        /// <summary>
        /// Obtém o resumo dos indicadores para gráficos do talhão.
        /// Retorna indicadores de acidez, saturação, equilíbrio de bases e participação na CTC.
        /// </summary>
        /// <param name="id">ID do relatório ou da coleta</param>
        /// <returns>Resumo do talhão com todos os indicadores para gráficos</returns>
        [HttpGet]
        [Route("{id}/indicadores-graficos")]
        public async Task<IActionResult> GetIndicadoresGraficos([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var resumo = await _relatorioService.GetResumoAcidezSoloAsync(id, userId);
                if (resumo == null)
                {
                    return NotFound(new { message = "Relatório não encontrado ou sem dados de análise." });
                }
                return Ok(resumo);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
