using api.coleta.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracaoPadraoController : BaseController
    {
        private readonly ConfiguracaoPadraoService _configuracaoPadraoService;
        private readonly INotificador _notificador;
        public ConfiguracaoPadraoController(ConfiguracaoPadraoService configuracaoPadraoService, INotificador notificador) : base(notificador)
        {
            _configuracaoPadraoService = configuracaoPadraoService;
            _notificador = notificador;
        }

        [HttpGet]
        public IActionResult ListarConfiguracoes()
        {
            var configuracoes = _configuracaoPadraoService.ListarConfiguracoes();
            return Ok(configuracoes);
        }
    }
}