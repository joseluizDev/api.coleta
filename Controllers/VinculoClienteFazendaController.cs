using api.vinculoClienteFazenda.Models.DTOs;
using api.vinculoClienteFazenda.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.vinculoClienteFazenda.Controllers
{
   [ApiController]
   [Route("vinculo-cliente-fazenda")]
   public class VinculoClienteFazendaController : ControllerBase
   {
      private readonly VinculoClienteFazendaService _vinculoService;

      public VinculoClienteFazendaController(VinculoClienteFazendaService vinculoService)
      {
         _vinculoService = vinculoService;
      }

      [HttpGet]
      [Route("buscar")]
      public IActionResult BuscarVinculoPorId(Guid id)
      {
         var vinculo = _vinculoService.BuscarVinculoPorId(id);
         if (vinculo == null)
            return NotFound("Vínculo não encontrado");
         return Ok(vinculo);
      }

      [HttpPost]
      [Route("salvar")]
      public IActionResult SalvarVinculos([FromBody] VinculoRequestDTO vinculos)
      {
         _vinculoService.SalvarVinculo(vinculos);
         return Ok();
      }

      [HttpPut]
      [Route("atualizar")]
      public IActionResult AtualizarVinculo([FromBody] VinculoRequestDTO vinculo)
      {
         _vinculoService.AtualizarVinculo(vinculo);
         return Ok();
      }

      [HttpDelete]
      [Route("deletar")]
      public IActionResult DeletarVinculo(Guid id)
      {
         _vinculoService.DeletarVinculo(id);
         return Ok();
      }
   }
}
