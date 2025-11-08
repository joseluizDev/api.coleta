using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.talhao.Models.DTOs;
using api.talhao.Services;
using Microsoft.AspNetCore.Authorization;
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
        public IActionResult ListarTalhao([FromQuery] QueryTalhao query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhao = _talhaoService.ListarTalhao(userId, query);
                return Ok(talhao);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("BuscarTalhaoPorFazendaID")]
        [Authorize]
        public IActionResult BuscarTalhaoPorFazendaID([FromQuery] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhao = _talhaoService.BuscarTalhaoPorFazendaID(userId, id);
                return Ok(talhao);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("listar-agrupado-por-fazenda")]
        [Authorize]
        public IActionResult ListarTalhoesAgrupadosPorFazenda([FromQuery] Guid? fazendaId = null)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var talhoes = _talhaoService.ListarTalhoesAgrupadosPorFazenda(userId, fazendaId);
                return Ok(talhoes);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpDelete]
        [Route("deletar-talhao-json/{id}")]
        public IActionResult DeletarTalhaoJson(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            
            if (userId != Guid.Empty)
            {
                var resultado = _talhaoService.DeletarTalhaoJson(userId, id);
                if (resultado)
                {
                    return Ok(new { message = "TalhaoJson deletado com sucesso" });
                }
                return NotFound(new { message = "TalhaoJson não encontrado ou você não tem permissão para deletá-lo" });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("atualizar-nome-talhao-json/{id}")]
        public IActionResult AtualizarNomeTalhaoJson(Guid id, [FromBody] AtualizarNomeTalhaoJsonDTO dto)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            
            if (userId != Guid.Empty)
            {
                var resultado = _talhaoService.AtualizarNomeTalhaoJson(userId, id, dto);
                if (resultado)
                {
                    return Ok(new { message = "Nome do TalhaoJson atualizado com sucesso" });
                }
                return NotFound(new { message = "TalhaoJson não encontrado ou você não tem permissão para atualizá-lo" });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
        
    }
}