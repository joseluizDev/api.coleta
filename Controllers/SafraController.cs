using api.safra.Models.DTOs;
using api.safra.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.safra.Controllers
{
   [ApiController]
   [Route("safra")]
   public class SafraController : ControllerBase
   {
      private readonly SafraService _safraService;

      public SafraController(SafraService safraService)
      {
         _safraService = safraService;
      }

      [HttpGet]
      [Route("buscar")]
      public IActionResult BuscarSafraPorId(Guid id)
      {
         var safra = _safraService.BuscarSafraPorId(id);
         if (safra == null)
            return NotFound("Safra não encontrada");
         return Ok(safra);
      }

      [HttpPost]
      [Route("salvar")]
      public IActionResult SalvarSafras([FromBody] SafraRequestDTO safras)
      {
         _safraService.SalvarSafra(safras);
         return Ok();
      }

      [HttpPut]
      [Route("atualizar")]
      public IActionResult AtualizarSafra([FromBody] SafraRequestDTO safra)
      {
         _safraService.AtualizarSafra(safra);
         return Ok();
      }

      [HttpDelete]
      [Route("deletar")]
      public IActionResult DeletarSafra(Guid id)
      {
         _safraService.DeletarSafra(id);
         return Ok();
      }
   }
}
