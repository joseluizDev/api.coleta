using System.Security.Claims;
using api.cliente.Models.DTOs;
using api.cliente.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.cliente.Controllers
{
   [ApiController]
   [Route("cliente")]
   public class ClienteController : ControllerBase
   {
      private readonly ClienteService _clienteService;

      public ClienteController(ClienteService clienteService)
      {
         _clienteService = clienteService;
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

      [HttpPost]
      [Route("salvar")]
      [Authorize]
      public IActionResult SalvarClientes([FromBody] ClienteRequestDTO clientes)
      {

            try
            {
                var userIdClam = HttpContext.User.FindFirst(ClaimTypes.Name);
                if (userIdClam == null || string.IsNullOrWhiteSpace(userIdClam.Value))
                {
                    return BadRequest("Reinvindicação de ID de usuário não encontrado ou inválido.");
                }
                if (!Guid.TryParse(userIdClam.Value, out var userIdGuid))
                {
                    return BadRequest("Id de usuário inválido.");
                }

                _clienteService.AtualizarCliente(clientes);
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
