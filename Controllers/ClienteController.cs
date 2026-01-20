using System.Security.Claims;
using api.cliente.Interfaces;
using api.cliente.Models.DTOs;
using api.cliente.Services;
using api.coleta.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.cliente.Controllers
{
    [ApiController]
    [Route("api/cliente")]
    public class ClienteController : BaseController
    {
        private readonly ClienteService _clienteService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public ClienteController(ClienteService clienteService, INotificador
            notificador, IJwtToken jwtToken) : base(notificador)
        {
            _clienteService = clienteService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet("listar")]
        [Authorize]
        public IActionResult ListarClientes([FromQuery] QueryClienteDTO query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var response = _clienteService.TotalClientes(userId, query);
                return Ok(response);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });

        }

        [HttpGet]
        [Route("buscar")]
        [Authorize]
        public IActionResult BuscarClientePorId(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var response = _clienteService.BuscarClientePorId(userId, id);
                if (response != null)
                {
                    return Ok(response);
                }
                return NotFound(new { message = "Cliente não encontrado." });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPost("salvar")]
        [Authorize]
        public IActionResult SalvarClientes([FromBody] ClienteRequestDTO clientes)
        {

            try
            {
                var token = ObterIDDoToken();
                Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null)
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");

                var cliente = _clienteService.SalvarCliente(clientes, userId);
                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPut]
        [Route("atualizar")]
        [Authorize]
        public IActionResult AtualizarCliente([FromBody] ClienteRequestDTO cliente)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var c = _clienteService.AtualizarCliente(userId, cliente);
                if (c != null)
                {
                    return Ok(c);
                }
                return NotFound(new { message = "Cliente não encontrado." });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpDelete]
        [Route("deletar")]
        [Authorize]
        public IActionResult DeletarCliente(Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var cliente = _clienteService.DeletarCliente(userId, id);
                return Ok(cliente);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public IActionResult ListarTodosClientes()
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var clientes = _clienteService.ListarTodosClientes(userId);
                return Ok(clientes);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
