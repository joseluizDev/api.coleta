using api.funcionario.Models.DTOs;
using api.funcionario.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.funcionario.Controllers
{
   [ApiController]
   [Route("api/funcionario")]
   public class FuncionarioController : ControllerBase
   {
      private readonly FuncionarioService _funcionarioService;

      public FuncionarioController(FuncionarioService funcionarioService)
      {
         _funcionarioService = funcionarioService;
      }

      [HttpGet]
      [Route("buscar")]
      public IActionResult BuscarFuncionarioPorId(Guid id)
      {
         var funcionario = _funcionarioService.BuscarFuncionarioPorId(id);
         if (funcionario == null)
            return NotFound("Funcionário não encontrado");
         return Ok(funcionario);
      }

      [HttpPost]
      [Route("salvar")]
      public IActionResult SalvarFuncionarios([FromBody] FuncionarioRequestDTO funcionarios)
      {
         _funcionarioService.SalvarFuncionario(funcionarios);
         return Ok();
      }

      [HttpPut]
      [Route("atualizar")]
      public IActionResult AtualizarFuncionario([FromBody] FuncionarioRequestDTO funcionario)
      {
         _funcionarioService.AtualizarFuncionario(funcionario);
         return Ok();
      }

      [HttpDelete]
      [Route("deletar")]
      public IActionResult DeletarFuncionario(Guid id)
      {
         _funcionarioService.DeletarFuncionario(id);
         return Ok();
      }
   }
}
