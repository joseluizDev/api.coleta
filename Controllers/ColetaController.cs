using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using api.cliente.Interfaces;
using api.coleta.repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using api.coleta.models.dtos;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/coleta")]
    public class ColetaController : BaseController
    {
        private readonly ColetaService _coletaService;
        private readonly IJwtToken _jwtToken;


        public ColetaController(ColetaService coletaService, IJwtToken jwtToken, INotificador notificador) : base(notificador)
        {
            _coletaService = coletaService;
            _jwtToken = jwtToken;
        }
        [HttpGet]
        [Route("listar")]
        [Authorize]
        public async Task<IActionResult> BuscarColetas([FromQuery] QueryColeta query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);

            if (userId != Guid.Empty)
            {
                // Usando await no método assíncrono
                var coleta = await _coletaService.BucarColetasPorUsuarioAsync(userId, query);
                return Ok(coleta);
            }

            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }


    }
}