using api.coleta.repositories;
using api.coleta.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;


namespace api.coleta.Controllers
{
    [ApiController]
    [Route("coleta")]
    public class ColetasController : ControllerBase
    {
        private readonly ColetaService _coletaService;

        private readonly INotificador _notificador;

        public ColetasController(ColetaService coletaService, INotificador notificador)
        {
            _coletaService = coletaService;
            _notificador = notificador;
        }


        //[HttpGet("busca")]
        //[OutputCache(Duration = 2592000)]
        //public async Task<IActionResult> BuscarColetas([FromHeader] string nomeCidade)
        //{
        //    try
        //    {
        //        var result = await _coletaService.BuscarColetasCidade(nomeCidade);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Erro = ex.Message });
        //    }
        //}
    }
}
