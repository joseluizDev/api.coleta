using api.coleta.Controllers;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Mvc;
using api.cliente.Interfaces;

namespace api.ndvi.Controllers
{
    [ApiController]
    [Route("api/ndvi")]
    public class NdviController : BaseController
    {
        private readonly ImagemNdviService _imagemNdviService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public NdviController(IJwtToken jwtToken, INotificador notificador, ImagemNdviService imagemNdviService) : base(notificador)
        {
            _jwtToken = jwtToken;
            _notificador = notificador;
            _imagemNdviService = imagemNdviService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] ImagemNdviUploadDTO dto)
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userIdNullable == null) return BadRequest(new { message = "Token inválido" });
            Guid userId = (Guid)userIdNullable;
            Console.WriteLine(dto);
            var resultado = await _imagemNdviService.SalvarImagemAsync(dto, userId);
            if (resultado == null) return BadRequest(new { message = "Erro ao salvar imagem NDVI" });
            return Ok(resultado);
        }

        [HttpGet("{talhaoId}")]
        public async Task<IActionResult> GetByTalhaoId(Guid talhaoId)
        {
            var imagens = await _imagemNdviService.GetByTalhaoIdAsync(talhaoId);
            return Ok(imagens);
        }
    }
}
