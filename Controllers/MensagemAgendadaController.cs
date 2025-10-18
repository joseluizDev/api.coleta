using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/mensagens")]
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
        public IActionResult CriarMensagem([FromBody] MensagemAgendadaRequestDTO request)
        {

            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);
            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }
            request.UsuarioId = usuarioId.Value;
            try
            {
                _service.CriarMensagemAgendada(request);
                return CustomResponse();
            }
            catch
            {
                return BadRequest(new { message = "Erro ao processar o token." });
            }
        }

        [HttpGet]
        public IActionResult ObterTodas()
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var mensagens = _service.ObterMensagensPorUsuario(usuarioId.Value);
            return CustomResponse(mensagens);
        }

        [HttpGet("estatisticas")]
        public IActionResult ObterEstatisticas()
        {
            var token = ObterIDDoToken();
            var usuarioId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (usuarioId == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }

            var estatisticas = _service.ObterEstatisticas(usuarioId.Value);
            return CustomResponse(estatisticas);
        }
    }
}
