using Microsoft.AspNetCore.Mvc;
using api.vinculoClienteFazenda.Services;
using System.Text.Json;
using api.utils.DTOs;
using api.coleta.Models.DTOs;

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
                // Log para debug
                Console.WriteLine($"[API] Recebida requisição para gerar hexágonos: {request.Hectares} ha");

                var result = _utilsService.GenerateHexagons(request.Polygon, request.Hectares);

                Console.WriteLine($"[API] Hexágonos gerados com sucesso");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API ERROR] {ex.Message}");
                Console.WriteLine($"[API ERROR] StackTrace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message, details = ex.StackTrace });
            }
        }


        [HttpPost("get-points-area")]
        public IActionResult GetPointsInsideArea([FromBody] PontosDentroDaAreaRequest request)
        {
            try
            {
                var result = _utilsService.GetPointsInsideArea(request);
                if (result.Points.GetArrayLength() == 0)
                {
                    return BadRequest(new { error = "Nenhum ponto foi gerado dentro da área especificada." });
                }

                // Mantém compatibilidade: retorna apenas o array de pontos
                return Ok(result.Points);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}