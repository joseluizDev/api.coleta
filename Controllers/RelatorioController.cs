using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using api.safra.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/relatorio")]
    public class RelatorioController : BaseController
    {
        private readonly RelatorioService _relatorioService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public RelatorioController(IJwtToken jwtToken, INotificador notificador, RelatorioService relatorioService) : base(notificador)
        {
            _jwtToken = jwtToken;
            _notificador = notificador;
            _relatorioService = relatorioService;
        }


        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadData([FromForm] RelatorioDTO relatorio)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                string url = await _relatorioService.SalvarRelatorio(relatorio, userId);
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest(new { message = "Erro ao salvar o relatório." });
                }
                return Ok(new { message = "Upload realizado com sucesso!" });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetRelatorio([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var relatorio = await _relatorioService.GetRelario(id, userId);


                return Ok(relatorio);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("buscar")]
        public async Task<IActionResult> ListarRelatoriosPorUpload()
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var relatorios = await _relatorioService.ListarRelatoriosPorUploadAsync(userId);
                return Ok(relatorios);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
