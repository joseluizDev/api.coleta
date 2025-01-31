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
   [Route("cliente")]
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
        public IActionResult ListarClientes(
          [FromQuery] int? page,
          [FromQuery] int? limit
      )
      {
            try {
                var token = ObterIDDoToken();
                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null)
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");

                List<ClienteResponseDTO> clientes = _clienteService.BuscarClientesPaginados(
                    userId.Value, page ?? 0, limit ?? 0  
                );
                return Ok(clientes);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

      }

      [HttpGet]
      [Route("buscar")]
      public IActionResult BuscarClientePorId(Guid id)
      {
         var cliente = _clienteService.BuscarClientePorId(id);
         if (cliente == null)
            return NotFound("Cliente não encontrado");
         return Ok(cliente);
      }

      [HttpPost("salvar")]
      [Authorize]
      public IActionResult SalvarClientes([FromBody] ClienteRequestDTO clientes)
      {

            try
            {
                var token = ObterIDDoToken();
                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null)
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");
                
                _clienteService.SalvarCliente(clientes, userId.Value);
                return Ok();
            }
            catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }

      }

      [HttpPut]
      [Route("atualizar")]
      public IActionResult AtualizarCliente([FromBody] ClienteRequestDTO cliente)
      {
         _clienteService.AtualizarCliente(cliente);
         return Ok();
      }

      [HttpDelete]
      [Route("deletar")]
      public IActionResult DeletarCliente(Guid id)
      {
         _clienteService.DeletarCliente(id);
         return Ok();
      }
   }
}
