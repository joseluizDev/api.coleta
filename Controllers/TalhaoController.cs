using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.talhao.Models.DTOs;
using api.talhao.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.talhao.Controllers
{
    [ApiController]
    [Route("api/talhao")]
    public class TalhaoController : BaseController
    {
        private readonly TalhaoService _talhaoService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public TalhaoController(TalhaoService talhaoService, INotificador notificador, IJwtToken jwtToken) : base(notificador)
        {
            _talhaoService = talhaoService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet]
        [Route("buscar")]
        public IActionResult BuscarTalhaoPorId(Guid id)
        {
            var talhao = _talhaoService.BuscarTalhaoPorId(id);
            if (talhao == null)
                return NotFound("Talhão não encontrado");
            return Ok(talhao);
        }

        [HttpPost]
        [Route("salvar")]
        public IActionResult SalvarTalhoes([FromBody] TalhaoRequestDTO talhoes)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhao = _talhaoService.SalvarTalhoes(userId, talhoes);
                return Ok(talhao);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("atualizar")]
        public IActionResult AtualizarTalhao([FromBody] TalhaoRequestDTO talhao)
        {
            _talhaoService.AtualizarTalhao(talhao);
            return Ok();
        }

        [HttpDelete]
        [Route("deletar")]
        public IActionResult DeletarTalhao(Guid id)
        {
            _talhaoService.DeletarTalhao(id);
            return Ok();
        }

        [HttpGet]
        [Route("listar")]
        public IActionResult ListarTalhao([FromQuery] int page)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhao = _talhaoService.ListarTalhao(userId, page);
                return Ok(talhao);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}