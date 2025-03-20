﻿using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using api.safra.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/visualizar-mapa")]
    public class VisualizarMapaController : BaseController
    {
        private readonly VisualizarMapaService _visualizarMapaService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public VisualizarMapaController(VisualizarMapaService visualizarMapaService, INotificador notificador, IJwtToken jwtToken)
            : base(notificador)
        {
            _visualizarMapaService = visualizarMapaService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpPost("salvar")]
        public IActionResult Salvar([FromBody] VisualizarMapInputDto visualizarMapa)
        {
            try
            {
                var token = ObterIDDoToken();
                Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
                var visualizarMapaSalvo = _visualizarMapaService.Salvar(userId, visualizarMapa);
                if (visualizarMapaSalvo == null)
                    return BadRequest("Erro ao salvar visualização de mapa.");
                return Ok(visualizarMapaSalvo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao salvar a visualização de mapa: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int page)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safras = _visualizarMapaService.listar(userId, page);
                return Ok(safras);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

    }
}
