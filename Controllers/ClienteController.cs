using System.Security.Claims;
using api.cliente.Interfaces;
using api.cliente.Models.DTOs;
using api.cliente.Services;
using api.coleta.Controllers;
using AutoMapper;
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
        [ProducesResponseType(typeof(ClienteResponsePaginacaoDto), 200, "Application/json")]
        [ProducesResponseType(typeof(string), 400, "text/plain")]
        public IActionResult ListarClientes(
          [FromQuery] int? page,
          [FromQuery] int? limit,
          [FromQuery] string? searchTerm
      )
        {
            try
            {
                var token = ObterIDDoToken();
                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null)
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");

                var totalClientes = _clienteService.TotalClientes(userId.Value, searchTerm ?? "");

                double totalPages = 1;

                if (limit != null && limit > totalClientes)
                {
                    return StatusCode(400, "Limite inválido");
                }


                if (limit != null && limit > 0 && limit <= totalClientes)
                {
                    totalPages = totalClientes / limit.Value;
                    totalPages = Math.Ceiling(totalPages);
                }

                if (page != null && (page > totalPages || page < 1))
                {
                    return StatusCode(400, "Página inválida.");
                }

                bool hasNextPage = page != null && page.Value < totalPages;
                bool hasPreviousPage = page != null && page.Value > 1;


                List<ClienteResponseDTO> clientes = _clienteService.BuscarClientes(
                    userId.Value,
                    page ?? 0,
                    limit ?? 0,
                    searchTerm ?? ""
                );

                var jsonResponse = new
                {
                    paginaAtual = page ?? 1,
                    tamanhoPagina = limit ?? 10,
                    total = totalClientes,
                    totalPaginas = totalPages,
                    proximaPagina = hasNextPage,
                    paginaAnterior = hasPreviousPage,
                    clientes = clientes
                };

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet]
        [Route("buscar")]
        public IActionResult BuscarClientePorId(Guid id)
        {

            string token = ObterIDDoToken();
            Guid? userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
                return BadRequest("Token inválido ou ID do usuário não encontrado.");

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
            catch (Exception ex)
            {
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
