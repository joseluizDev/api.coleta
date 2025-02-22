using Microsoft.AspNetCore.Mvc;
using api.vinculoClienteFazenda.Services;
using System.Text.Json;
using api.utils.DTOs;

namespace api.utils.Controllers
{
    [ApiController]
    [Route("api/utils")]
    public class UtilsController : ControllerBase
    {
        private readonly UtilsService _utilsService;

        public UtilsController(UtilsService utilsService)
        {
            _utilsService = utilsService;
        }

        [HttpPost("generate-hexagons")]
        public IActionResult GenerateHexagons([FromBody] HexagonRequestDto request)
        {
            try
            {
                var result = _utilsService.GenerateHexagons(request.Polygon, request.Hectares);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}