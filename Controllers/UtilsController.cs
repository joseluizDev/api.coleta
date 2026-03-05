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

        [HttpGet("cultivos-cultivares")]
        public IActionResult GetCultivosCultivares()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "cultivos_cultivares.json");
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { error = "Arquivo de cultivos/cultivares não encontrado." });

                var json = System.IO.File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, List<JsonElement>>>(json);
                if (data == null)
                    return BadRequest(new { error = "Erro ao processar cultivos/cultivares." });

                var result = data.Keys
                    .OrderBy(k => k)
                    .Select(cultivo => new
                    {
                        cultivo,
                        cultivares = data[cultivo]
                            .Select(c =>
                            {
                                if (!c.TryGetProperty("CULTIVAR", out var v)) return null;
                                var str = v.ValueKind == JsonValueKind.String
                                    ? v.GetString()
                                    : v.ToString();
                                return str?.Trim();
                            })
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct()
                            .OrderBy(s => s)
                            .ToList()
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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