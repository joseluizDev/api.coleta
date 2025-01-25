using api.coleta.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using api.coleta.Controllers;
using api.cliente.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly INotificador _notificador;

        public UsuarioController(UsuarioService usuarioService, INotificador notificador, IJwtToken jwtToken)
        {

            _usuarioService = usuarioService;
            _notificador = notificador;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login([FromQuery] UsuarioLoginDTO usuarioLogin)
        {
            var usuario = _usuarioService.Login(usuarioLogin.Email, usuarioLogin.Senha);
            if (usuario == null)
                return NotFound("Usuário não encontrado");
            return Ok(

                new
                {
                    Token = usuario
                }
            );
        }

        [HttpPost]
        [Route("cadastrar")]
        public IActionResult Cadastrar([FromBody] UsuarioResquestDTO usuario)
        {
            var usuarioCadastrado = _usuarioService.Cadastrar(usuario);
            if (usuarioCadastrado == null)
                return BadRequest("Erro ao cadastrar usuário");
            return Ok(usuarioCadastrado);
        }

        [HttpGet]
        [Route("buscar")]
        [Authorize]
        public IActionResult BuscarUsuarioPorId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.Name);
            if (userIdClaim == null)
                return BadRequest("Usuário não encontrado");
            var userId = userIdClaim.Value;
            if (!Guid.TryParse(userId, out var userIdGuid))
                return BadRequest("ID de usuário inválido");
            var usuario = _usuarioService.BuscarUsuarioPorId(userIdGuid);
            return Ok(usuario);
        }
    }
}
