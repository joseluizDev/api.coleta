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
            try
            {
                var token = ObterIDDoToken();
                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null) return BadRequest(new { message = "Token inválido" });
                Guid userId = (Guid)userIdNullable;

                Console.WriteLine($"[NDVI Upload] TalhaoId: {dto.TalhaoId}, TipoImagem: {dto.TipoImagem}, Arquivo: {dto.Arquivo?.FileName}");

                var resultado = await _imagemNdviService.SalvarImagemAsync(dto, userId);
                if (resultado == null) return BadRequest(new { message = "Erro ao salvar imagem - arquivo vazio ou upload falhou" });

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NDVI Upload] Erro: {ex.Message}");
                Console.WriteLine($"[NDVI Upload] Stack: {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro interno ao processar upload", error = ex.Message });
            }
        }

        [HttpGet("{talhaoId}")]
        public async Task<IActionResult> GetByTalhaoId(Guid talhaoId)
        {
            var imagens = await _imagemNdviService.GetByTalhaoIdAsync(talhaoId);
            return Ok(imagens);
        }

        [HttpDelete("image/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var token = ObterIDDoToken();
                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null) return BadRequest(new { message = "Token inválido" });

                var resultado = await _imagemNdviService.DeletarImagemAsync(id);
                if (!resultado) return NotFound(new { message = "Imagem não encontrada" });

                return Ok(new { message = "Imagem deletada com sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NDVI Delete] Erro: {ex.Message}");
                return StatusCode(500, new { message = "Erro ao deletar imagem", error = ex.Message });
            }
        }
    }
}
