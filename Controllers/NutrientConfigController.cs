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
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public NutrientConfigController(
            NutrientConfigService nutrientConfigService,
            INotificador notificador,
            IJwtToken jwtToken) : base(notificador)
        {
            _nutrientConfigService = nutrientConfigService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet]
        [Authorize]
        public IActionResult ListarNutrientConfigs()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var configs = _nutrientConfigService.ListarNutrientConfigs(userId);
            return Ok(configs);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult BuscarNutrientConfig(Guid id)
        {
            var config = _nutrientConfigService.BuscarNutrientConfigPorId(id);
            if (config == null)
            {
                return NotFound(new { message = "Configuração não encontrada." });
            }
            return Ok(config);
        }

        [HttpGet("fallback/{nutrientName}")]
        [Authorize]
        public IActionResult BuscarNutrientConfigComFallback(string nutrientName)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var config = _nutrientConfigService.BuscarNutrientConfigComFallback(nutrientName, userId);
            if (config == null)
            {
                return NotFound(new { message = "Configuração não encontrada." });
            }
            return Ok(config);
        }

        [HttpPost]
        [Authorize]
        public IActionResult SalvarNutrientConfig([FromBody] NutrientConfigRequestDTO configDTO)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            var config = _nutrientConfigService.SalvarNutrientConfig(configDTO, userId);
            return CreatedAtAction(nameof(BuscarNutrientConfig), new { id = config.Id }, config);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult AtualizarNutrientConfig(Guid id, [FromBody] NutrientConfigRequestDTO configDTO)
        {
            try
            {
                var config = _nutrientConfigService.AtualizarNutrientConfig(id, configDTO);
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeletarNutrientConfig(Guid id)
        {
            _nutrientConfigService.DeletarNutrientConfig(id);
            return NoContent();
        }

        [HttpGet("global")]
        [Authorize]
        public IActionResult ListarGlobais()
        {
            var configs = _nutrientConfigService.ListarGlobais();
            return Ok(configs);
        }

        [HttpPost("global")]
        [Authorize]
        public IActionResult SalvarGlobal([FromBody] NutrientConfigRequestDTO configDTO)
        {
            configDTO.UserId = null; // Força global
            var config = _nutrientConfigService.SalvarNutrientConfig(configDTO, null);
            return CreatedAtAction(nameof(ListarGlobais), config);
        }

        [HttpPut("global/{id}")]
        [Authorize]
        public IActionResult AtualizarGlobal(Guid id, [FromBody] NutrientConfigRequestDTO configDTO)
        {
            configDTO.UserId = null;
            var config = _nutrientConfigService.AtualizarNutrientConfig(id, configDTO);
            return Ok(config);
        }

        [HttpDelete("global/{id}")]
        [Authorize]
        public IActionResult DeletarGlobal(Guid id)
        {
            _nutrientConfigService.DeletarGlobal(id);
            return NoContent();
        }

        [HttpGet("personal")]
        [Authorize]
        public IActionResult ListarPersonalizadas()
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token)!;
            var configs = _nutrientConfigService.ListarPersonalizadas(userId);
            return Ok(configs);
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
            return CreatedAtAction(nameof(ListarPersonalizadas), config);
        }

        [HttpPut("personal/{id}")]
        [Authorize]
        public IActionResult AtualizarPersonalizada(Guid id, [FromBody] NutrientConfigRequestDTO configDTO)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token)!;
            configDTO.UserId = userId;
            var config = _nutrientConfigService.AtualizarNutrientConfig(id, configDTO);
            return Ok(config);
        }

        [HttpDelete("personal/{id}")]
        [Authorize]
        public IActionResult DeletarPersonalizada(Guid id)
        {
            var token = ObterIDDoToken();
            if (token == null)
            {
                return BadRequest(new { message = "Token inválido." });
            }

            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token)!;
            _nutrientConfigService.DeletarPersonalizada(id, userId);
            return NoContent();
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