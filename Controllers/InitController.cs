using api.coleta.repositories;
using api.coleta.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;


namespace api.init.Controllers
{
   [ApiController]
   [Route("/")]
    public class InitController : ControllerBase
    {
        [HttpGet]
        [Route("/")]
        public IActionResult Listar()
        {
            return Ok();
        }
    }
}
