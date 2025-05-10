using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.safra.Models.DTOs;
using api.safra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.safra.Controllers
{
    [ApiController]
    [Route("api/safra")]
    public class SafraController : BaseController
    {
        private readonly SafraService _safraService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public SafraController(SafraService safraService, IJwtToken jwtToken, INotificador notificador) : base(notificador)
        {
            _safraService = safraService;
            _jwtToken = jwtToken;
            _notificador = notificador;
        }

        [HttpGet]
        [Route("buscar")]
        [Authorize]
        public IActionResult BuscarSafraPorId(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safra = _safraService.BuscarSafraPorId(userId, id);
                return Ok(safra);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });

        }

        [HttpPost]
        [Route("salvar")]
        [Authorize]
        public IActionResult SalvarSafras([FromBody] SafraRequestDTO safras)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safra = _safraService.SalvarSafra(userId, safras);
                return Ok(safra);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("atualizar")]
        [Authorize]
        public IActionResult AtualizarSafra([FromBody] SafraRequestDTO safra)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var s = _safraService.AtualizarSafra(userId, safra);
                if (s != null)
                {
                    return Ok(s);
                }
                return NotFound(new { message = "Safra não encontrada." });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpDelete]
        [Route("deletar")]
        [Authorize]
        public IActionResult DeletarSafra(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safra = _safraService.DeletarSafra(userId, id);
                return Ok(safra);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("listar")]
        [Authorize]
        public IActionResult ListarSafras([FromQuery] QuerySafra query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safras = _safraService.ListarSafra(userId, query);
                return Ok(safras);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
