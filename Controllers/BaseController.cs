using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{

    public abstract class BaseController : ControllerBase
    {
        protected readonly INotificador Notificador;

        protected BaseController(INotificador notificador)
        {
            Notificador = notificador;
        }

        protected IActionResult CustomResponse(object result = null)
        {
            if (Notificador.TemNotificacao())
            {
                return BadRequest(new { errors = Notificador.ObterNotificacoes() });
            }

            return Ok(result);
        }
        protected string ObterTokenDoHeader()
        {
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
            return authorizationHeader.StartsWith("Bearer ")
                ? authorizationHeader.Substring("Bearer ".Length).Trim()
                : authorizationHeader;
        }
    }
}