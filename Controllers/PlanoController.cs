using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/plano")]
    public class PlanoController : BaseController
    {
        private readonly PlanoService _planoService;

        public PlanoController(
            PlanoService planoService,
            INotificador notificador) : base(notificador)
        {
            _planoService = planoService;
        }

        /// <summary>
        /// Lista todos os planos ativos (público)
        /// </summary>
        [HttpGet("listar")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarPlanos()
        {
            var planos = await _planoService.ListarPlanosAtivosAsync();
            return Ok(planos);
        }

        /// <summary>
        /// Obtém um plano por ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPlano([FromRoute] Guid id)
        {
            var plano = await _planoService.ObterPorIdAsync(id);

            if (plano == null)
            {
                return NotFound("Plano não encontrado");
            }

            return Ok(plano);
        }

        /// <summary>
        /// Cria um novo plano (admin only)
        /// </summary>
        [HttpPost("criar")]
        [Authorize]
        public async Task<IActionResult> CriarPlano([FromBody] PlanoCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plano = await _planoService.CriarPlanoAsync(dto);
            return CustomResponse(plano);
        }

        /// <summary>
        /// Atualiza um plano existente (admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> AtualizarPlano([FromRoute] Guid id, [FromBody] PlanoCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plano = await _planoService.AtualizarPlanoAsync(id, dto);
            return CustomResponse(plano);
        }

        /// <summary>
        /// Desativa um plano (admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DesativarPlano([FromRoute] Guid id)
        {
            var resultado = await _planoService.DesativarPlanoAsync(id);
            return CustomResponse(resultado);
        }
    }
}
