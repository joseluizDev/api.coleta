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

        //GetPointsInsideArea
        [HttpPost("get-points-area")]
        //public IActionResult GetPointsInsideArea([FromBody] PontosDentro request)
        //{
        //    try
        //    {
        //        var result = _utilsService.GetPointsInsideArea(request);

        //        // Verifica se conseguiu gerar pontos
        //        if (result.GetArrayLength() == 0)
        //        {
        //            return BadRequest(new { error = "Nenhum ponto foi gerado dentro da área especificada." });
        //        }

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}
    }
}