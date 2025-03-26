using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.coleta.Utils;
using api.funcionario.Models.DTOs;
using api.funcionario.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.funcionario.Controllers
{
    [ApiController]
    [Route("api/funcionario")]
    public class FuncionarioController : BaseController
    {
        private readonly FuncionarioService _funcionarioService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public FuncionarioController(FuncionarioService funcionarioService, INotificador notificador,
            IJwtToken jwtToken) : base(notificador)
        {
            _funcionarioService = funcionarioService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet]
        [Route("buscar")]
        public IActionResult BuscarFuncionarioPorId(Guid id)
        {
            var funcionario = _funcionarioService.BuscarFuncionarioPorId(id);
            if (funcionario == null)
                return NotFound("Funcionário não encontrado");
            return Ok(funcionario);
        }

        [HttpPost]
        [Route("salvar")]
        public IActionResult SalvarFuncionarios([FromBody] FuncionarioRequestDTO funcionarios)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                _funcionarioService.SalvarFuncionario(userId, funcionarios);
                return Ok();
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("atualizar")]
        public IActionResult AtualizarFuncionario([FromBody] FuncionarioRequestDTO funcionario)
        {
            _funcionarioService.AtualizarFuncionario(funcionario);
            return Ok();
        }

        [HttpDelete]
        [Route("deletar")]
        public IActionResult DeletarFuncionario(Guid id)
        {
            _funcionarioService.DeletarFuncionario(id);
            return Ok();
        }

        [HttpGet]
        [Route("listar")]
        public IActionResult ListarFuncionarios([FromRoute] int page)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                PagedResult<FuncionarioResponseDTO> funcionarios = _funcionarioService.ListarFuncionarios(userId, page);
                return Ok(funcionarios);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}