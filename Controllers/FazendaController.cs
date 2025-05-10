using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.coleta.repositories;
using api.coleta.Repositories;
using api.fazenda.models;
using api.fazenda.Models.Entidades;
using api.fazenda.repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;


namespace api.fazenda.Controllers
{
    [ApiController]
    [Route("api/fazenda")]
    public class FazendaController : BaseController
    {
        private readonly FazendaService _fazendaService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public FazendaController(FazendaService fazendaService, INotificador notificador, IJwtToken jwtToken) : base(notificador)
        {
            _fazendaService = fazendaService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet]
        [Route("listar")]
        [Authorize]
        public IActionResult ListarFazendas([FromQuery] QueryFazenda query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var fazendas = _fazendaService.ListarFazendas(userId, query);
                return Ok(fazendas);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("buscar")]
        [Authorize]
        public IActionResult BuscarFazendaPorId(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var fazenda = _fazendaService.BuscarFazendaPorId(userId, id);
                if (fazenda != null)
                {
                    return Ok(fazenda);
                }
                return NotFound("Fazenda não encontrada");
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPost]
        [Route("salvar")]
        [Authorize]
        public IActionResult SalvarFazendas([FromBody] FazendaRequestDTO fazendas)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var fazenda = _fazendaService.SalvarFazendas(userId, fazendas);
                return Ok(fazenda);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("atualizar")]
        [Authorize]
        public IActionResult AtualizarFazenda([FromBody] FazendaRequestDTO fazenda)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var s = _fazendaService.AtualizarFazenda(userId, fazenda);
                if (s != null)
                {
                    return Ok(s);
                }
                return NotFound(new { message = "Fazenda não encontrada." });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpDelete]
        [Route("deletar")]
        [Authorize]
        public IActionResult DeletarFazenda(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var fazenda = _fazendaService.DeletarFazenda(userId, id);
                return Ok(fazenda);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public IActionResult ListarTodasFazendas()
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var fazendas = _fazendaService.ListarTodasFazendas(userId);
                return Ok(fazendas);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
