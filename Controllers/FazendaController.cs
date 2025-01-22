using api.coleta.repositories;
using api.coleta.Repositories;
using api.fazenda.models;
using api.fazenda.Models.Entidades;
using api.fazenda.repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;


namespace api.fazenda.Controllers
{
   [ApiController]
   [Route("fazenda")]
   public class FazendaController : ControllerBase
   {
      private readonly FazendaService _fazendaService;
      private readonly INotificador _notificador;

      public FazendaController(FazendaService fazendaService, INotificador notificador)
      {
         _fazendaService = fazendaService;
         _notificador = notificador;
      }

      [HttpGet]
      [Route("buscar")]
      public IActionResult BuscarFazendaPorId(Guid id)
      {
         var fazenda = _fazendaService.BuscarFazendaPorId(id);
         if (fazenda == null)
            return NotFound("Fazenda não encontrada");
         return Ok(fazenda);
      }

      [HttpPost]
      [Route("salvar")]
      public IActionResult SalvarFazendas([FromBody] FazendaRequestDTO fazendas)
      {
         _fazendaService.SalvarFazendas(fazendas);
         return Ok();
      }

      [HttpPut]
      [Route("atualizar")]
      public IActionResult AtualizarFazenda([FromBody] FazendaRequestDTO fazenda)
      {
         _fazendaService.AtualizarFazenda(fazenda);
         return Ok();
      }

      [HttpDelete]
      [Route("deletar")]
      public IActionResult DeletarFazenda(Guid id)
      {
         _fazendaService.DeletarFazenda(id);
         return Ok();
      }
   }
}
