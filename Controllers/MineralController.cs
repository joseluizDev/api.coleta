using api.cliente.Interfaces;
using api.coleta.Services;
using api.safra.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/mineral")]
    public class MineralController : BaseController
    {
        private readonly MineralService _mineralService;
        private readonly IJwtToken _jwtToken;
        private readonly INotificador _notificador;

        public MineralController(MineralService mineralService, IJwtToken jwtToken, INotificador notificador) : base(notificador)
        {
            _mineralService = mineralService;
            _jwtToken = jwtToken;
            _notificador = notificador;
        }

        [HttpGet]
        [Route("listar")]
        public IActionResult ListarMineiras()
        {
            var response = _mineralService.ListarMinerais();
            if (response != null)
            {
                return Ok(response);
            }
            return NotFound();
        }
    }
}
