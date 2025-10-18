using api.cliente.Interfaces;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/mensagem")]
    [Authorize]
    public class MensagemAgendadaMobileController : BaseController
    {
        private readonly IJwtToken _jwtToken;
        private readonly MensagemAgendadaService _service;

        public MensagemAgendadaMobileController(
            INotificador notificador,
            IJwtToken jwtToken,
            MensagemAgendadaService service) : base(notificador)
        {
            _jwtToken = jwtToken;
            _service = service;
        }

        /// <summary>
        /// Lista todas as mensagens do usuário logado
        /// </summary>
        [HttpGet]
        [Route("listar")]
        public async Task<IActionResult> ListarMensagens([FromQuery] bool apenasNaoLidas = false)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            var userId = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userId == null)
            {
                return BadRequest(new { message = "Usuário não encontrado no token." });
            }

            var mensagens = await _service.ObterMensagensDoUsuarioAsync(userId.Value, apenasNaoLidas);
            return CustomResponse(mensagens);
        }

        /// <summary>
        /// Busca mensagem por ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> BuscarPorId(Guid id)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            var userId = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userId == null)
            {
                return BadRequest(new { message = "Usuário não encontrado no token." });
            }

            var mensagem = await _service.ObterPorIdAsync(id);

            if (mensagem == null)
            {
                return NotFound(new { message = "Mensagem não encontrada." });
            }

            // Verifica se a mensagem pertence ao usuário
            if (mensagem.UsuarioId != userId.Value)
            {
                return Forbid();
            }

            return CustomResponse(mensagem);
        }

        /// <summary>
        /// Marca mensagem como lida
        /// </summary>
        [HttpPut]
        [Route("{id}/marcar-lida")]
        public async Task<IActionResult> MarcarComoLida(Guid id)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            var userId = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userId == null)
            {
                return BadRequest(new { message = "Usuário não encontrado no token." });
            }

            var sucesso = await _service.MarcarComoLidaAsync(id, userId.Value);

            if (!sucesso)
            {
                return CustomResponse(new { sucesso = false });
            }

            return CustomResponse(new { sucesso = true, message = "Mensagem marcada como lida com sucesso" });
        }

        /// <summary>
        /// Conta mensagens não lidas do usuário
        /// </summary>
        [HttpGet]
        [Route("nao-lidas/contar")]
        public async Task<IActionResult> ContarNaoLidas()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            var userId = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userId == null)
            {
                return BadRequest(new { message = "Usuário não encontrado no token." });
            }

            var mensagens = await _service.ObterMensagensDoUsuarioAsync(userId.Value, apenasNaoLidas: true);

            return CustomResponse(new
            {
                total = mensagens.Count,
                mensagens = mensagens.OrderByDescending(m => m.DataHoraEnvio).Take(10).ToList()
            });
        }

        /// <summary>
        /// Lista apenas mensagens enviadas (não lidas)
        /// </summary>
        [HttpGet]
        [Route("nao-lidas")]
        public async Task<IActionResult> ListarNaoLidas()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            var userId = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userId == null)
            {
                return BadRequest(new { message = "Usuário não encontrado no token." });
            }

            var mensagens = await _service.ObterMensagensDoUsuarioAsync(userId.Value, apenasNaoLidas: true);

            return CustomResponse(mensagens.OrderByDescending(m => m.DataHoraEnvio).ToList());
        }
    }
}
