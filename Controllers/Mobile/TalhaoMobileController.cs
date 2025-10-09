using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.talhao.Services;
using api.talhao.Models.DTOs;
using api.fazenda.models;
using api.fazenda.repositories;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/talhao")]
    public class TalhaoMobileController(
        TalhaoService talhaoService,
        FazendaService fazendaService,
        INotificador notificador,
        IJwtToken jwtToken) : BaseController(notificador)
    {
        private readonly TalhaoService _talhaoService = talhaoService;
        private readonly FazendaService _fazendaService = fazendaService;
        private readonly IJwtToken _jwtToken = jwtToken;



        [HttpGet("{id}")]
        public IActionResult BuscarTalhaoPorId(Guid id)
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token)) return BadRequest("Token não fornecido.");

                try
                {
                    if (!_jwtToken.ValidarToken(token)) return BadRequest("Token inválido.");
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null) return BadRequest("ID do usuário não encontrado no token.");

                var talhao = _talhaoService.BuscarTalhaoPorId(userId.Value, id);
                if (talhao == null) return NotFound("Talhão não encontrado.");

                return Ok(talhao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar o talhão: " + ex.Message);
            }
        }

        [HttpGet("fazendas-talhoes")]
        public IActionResult ListarFazendasComTalhoes()
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token)) return BadRequest("Token não fornecido.");

                try
                {
                    if (!_jwtToken.ValidarToken(token)) return BadRequest("Token inválido.");
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null) return BadRequest("ID do usuário não encontrado no token.");

                // Buscar fazendas com seus talhões em uma única consulta otimizada
                var fazendasComTalhoes = _fazendaService.ListarFazendasComTalhoesPorUsuarioOuAdmin(userId.Value);

                return Ok(fazendasComTalhoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar as fazendas com talhões: " + ex.Message);
            }
        }
    }
}