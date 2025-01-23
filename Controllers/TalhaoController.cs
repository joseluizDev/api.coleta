using api.talhao.Models.DTOs;
using api.talhao.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.talhao.Controllers
{
   [ApiController]
   [Route("talhao")]
   public class TalhaoController : ControllerBase
   {
      private readonly TalhaoService _talhaoService;

      public TalhaoController(TalhaoService talhaoService)
      {
         _talhaoService = talhaoService;
      }

      [HttpGet]
      [Route("buscar")]
      public IActionResult BuscarTalhaoPorId(Guid id)
      {
         var talhao = _talhaoService.BuscarTalhaoPorId(id);
         if (talhao == null)
            return NotFound("Talhão não encontrado");
         return Ok(talhao);
      }

      [HttpPost]
      [Route("salvar")]
      public IActionResult SalvarTalhoes([FromBody] TalhaoRequestDTO talhoes)
      {
         _talhaoService.SalvarTalhoes(talhoes);
         return Ok();
      }

      [HttpPut]
      [Route("atualizar")]
      public IActionResult AtualizarTalhao([FromBody] TalhaoRequestDTO talhao)
      {
         _talhaoService.AtualizarTalhao(talhao);
         return Ok();
      }

      [HttpDelete]
      [Route("deletar")]
      public IActionResult DeletarTalhao(Guid id)
      {
         _talhaoService.DeletarTalhao(id);
         return Ok();
      }
   }
}
