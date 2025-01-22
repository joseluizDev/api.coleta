using api.coleta.repositories;
using api.coleta.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;


namespace api.coleta.Controllers
{
    [ApiController]
    [Route("coleta")]
    public class ColetasController : ControllerBase
    {
        private readonly ColetaService _coletaService;
        private readonly INotificador _notificador;

        public ColetasController(ColetaService coletaService, INotificador notificador)
        {
            _coletaService = coletaService;
            _notificador = notificador;
        }
    }
}
