using System;
using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/nutrient-config")]
    public class NutrientConfigController : BaseController
    {
        private readonly NutrientConfigService _nutrientConfigService;
        private readonly IJwtToken _jwtToken;

        public NutrientConfigController(
            NutrientConfigService nutrientConfigService,
            INotificador notificador,
            IJwtToken jwtToken) : base(notificador)
        {
            _nutrientConfigService = nutrientConfigService;
            _jwtToken = jwtToken;
        }

        [HttpPost("personal")]
        [Authorize]
        public IActionResult SalvarPersonalizada([FromBody] NutrientConfigRequestDTO configDTO)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token)!;
            configDTO.UserId = userId;
            var config = _nutrientConfigService.SalvarNutrientConfig(configDTO, userId);
            return Ok(config);
        }

        [HttpGet("personal/by-name/{nutrientName}")]
        [Authorize]
        public IActionResult BuscarPersonalizadaPorNome(string nutrientName)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token)!;
            var config = _nutrientConfigService.BuscarPersonalizadaPorNome(nutrientName, userId);
            if (config == null)
            {
                return NotFound(new { message = "Configuração personalizada não encontrada." });
            }
            return Ok(config);
        }
    }
}