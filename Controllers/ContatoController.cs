using api.coleta.Models.DTOs;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/contato")]
    public class ContatoController : BaseController
    {
        private readonly ContatoService _contatoService;

        public ContatoController(
            ContatoService contatoService,
            INotificador notificador
        ) : base(notificador)
        {
            _contatoService = contatoService;
        }

        /// <summary>
        /// Endpoint público para envio de formulário de contato.
        /// Envia email de confirmação para o usuário e notificação para os administradores.
        /// </summary>
        /// <param name="dto">Dados do contato</param>
        /// <returns>Confirmação do recebimento</returns>
        [HttpPost("enviar")]
        [AllowAnonymous]
        public async Task<IActionResult> EnviarContato([FromBody] ContatoRequestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    foreach (var error in errors)
                    {
                        Notificador.Notificar(new Notificacao(error));
                    }

                    return CustomResponse();
                }

                var resultado = await _contatoService.SalvarContatoAsync(dto);

                return CustomResponse(new
                {
                    sucesso = true,
                    mensagem = "Contato recebido com sucesso! Entraremos em contato em breve.",
                    contato = resultado
                });
            }
            catch (Exception ex)
            {
                Notificador.Notificar(new Notificacao($"Erro ao processar contato: {ex.Message}"));
                return CustomResponse();
            }
        }

        /// <summary>
        /// Endpoint administrativo para listar contatos recebidos.
        /// Requer autenticação.
        /// </summary>
        /// <param name="page">Página (default: 1)</param>
        /// <param name="pageSize">Tamanho da página (default: 10, max: 100)</param>
        /// <returns>Lista de contatos paginada</returns>
        [HttpGet("listar")]
        [Authorize]
        public IActionResult ListarContatos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var contatos = _contatoService.ListarContatos(page, pageSize);
                var total = _contatoService.ContarContatos();

                return CustomResponse(new
                {
                    pagina = page,
                    tamanhoPagina = pageSize,
                    totalRegistros = total,
                    totalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                    contatos
                });
            }
            catch (Exception ex)
            {
                Notificador.Notificar(new Notificacao($"Erro ao listar contatos: {ex.Message}"));
                return CustomResponse();
            }
        }
    }
}
