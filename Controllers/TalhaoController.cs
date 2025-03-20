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
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhao = _talhaoService.BuscarTalhaoPorId(userId, id);
                if (talhao == null)
                    return NotFound("Talhão não encontrado");
                return Ok(talhao);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("buscarPorTalhao")]
        public IActionResult BuscarTalhaoPorTalhao(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhaoEncontrado = _talhaoService.BuscarTalhaoPorTalhao(userId, id);
                if (talhaoEncontrado == null)
                    return NotFound("Talhão não encontrado");
                return Ok(talhaoEncontrado);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
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
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var t = _talhaoService.AtualizarTalhao(userId, talhao);
                if (t != null)
                {
                    return Ok(t);
                }
                return NotFound("Talhão não encontrado");
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        //[HttpDelete]
        //[Route("deletar")]
        //public IActionResult DeletarTalhao(Guid id)
        //{
        //    _talhaoService.DeletarTalhao(id);
        //    return Ok();
        //}

        [HttpDelete]
        [Route("deletar")]
        public IActionResult DeletarTalhao([FromQuery] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhao = _talhaoService.DeletarTalhao(userId, id);
                if (talhao == null)
                    return NotFound("Talhão não encontrado");
                return Ok(new { message = "Talhão deletado com sucesso" });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
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