using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MensagemAgendadaController : BaseController
    {
        private readonly MensagemAgendadaService _service;
        private readonly IJwtToken _jwtToken;

        public MensagemAgendadaController(
            MensagemAgendadaService service,
            INotificador notificador,
            IJwtToken jwtToken) : base(notificador)
        {
            _service = service;
            _jwtToken = jwtToken;
        }

        [HttpPost]
        public async Task<IActionResult> CriarMensagem([FromBody] MensagemAgendadaRequestDTO request)
        {
            var mensagem = await _service.CriarMensagemAgendadaAsync(request);
            return CustomResponse(mensagem);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodas([FromQuery] MensagemAgendadaQueryDTO query)
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            // Força o filtro pelo usuário logado (admin que criou)
            query.UsuarioId = usuarioId.Value;

            var (mensagens, total) = await _service.ObterMensagensComFiltrosAsync(query);

            return CustomResponse(new
            {
                mensagens,
                total,
                pagina = query.Page ?? 1,
                tamanhoPagina = query.PageSize,
                totalPaginas = (int)Math.Ceiling(total / (double)query.PageSize)
            });
        }

        [HttpGet("todas")]
        public async Task<IActionResult> ObterTodasSemFiltro()
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var query = new MensagemAgendadaQueryDTO
            {
                UsuarioId = usuarioId.Value
            };

            var (mensagens, total) = await _service.ObterMensagensComFiltrosAsync(query);
            return CustomResponse(mensagens);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var mensagem = await _service.ObterPorIdAsync(id);

            if (mensagem == null)
                return NotFound();

            // Verifica se a mensagem pertence ao usuário logado (admin que criou)
            if (mensagem.UsuarioId != usuarioId.Value)
            {
                return Forbid();
            }

            return CustomResponse(mensagem);
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> ObterPorUsuario(Guid usuarioId)
        {
            var mensagens = await _service.ObterMensagensPorUsuarioAsync(usuarioId);
            return CustomResponse(mensagens);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] MensagemAgendadaRequestDTO request)
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            // Verifica se a mensagem existe e pertence ao usuário (admin que criou)
            var mensagemExistente = await _service.ObterPorIdAsync(id);
            if (mensagemExistente == null)
            {
                return NotFound(new { message = "Mensagem não encontrada." });
            }

            if (mensagemExistente.UsuarioId != usuarioId.Value)
            {
                return Forbid();
            }

            var sucesso = await _service.AtualizarMensagemAsync(id, request);
            return CustomResponse(new { sucesso });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            // Verifica se a mensagem existe e pertence ao usuário (admin que criou)
            var mensagemExistente = await _service.ObterPorIdAsync(id);
            if (mensagemExistente == null)
            {
                return NotFound(new { message = "Mensagem não encontrada." });
            }

            if (mensagemExistente.UsuarioId != usuarioId.Value)
            {
                return Forbid();
            }

            var sucesso = await _service.CancelarMensagemAsync(id);
            return CustomResponse(new { sucesso });
        }

        [HttpGet("estatisticas")]
        public async Task<IActionResult> ObterEstatisticas()
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var estatisticas = await _service.ObterEstatisticasAsync(usuarioId.Value);
            return CustomResponse(estatisticas);
        }
    }
}
