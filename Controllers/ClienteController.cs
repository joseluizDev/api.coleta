using api.cliente.Models.DTOs;
using api.cliente.Services;
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
      public IActionResult SalvarClientes([FromBody] ClienteRequestDTO clientes)
      {
         _clienteService.AtualizarCliente(clientes);
         return Ok();
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
