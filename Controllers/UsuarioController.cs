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
    public class UsuarioController : BaseController
    {
        private readonly UsuarioService _usuarioService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public UsuarioController(UsuarioService usuarioService, INotificador notificador, IJwtToken jwtToken)
            : base(notificador)
        {
            _usuarioService = usuarioService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] UsuarioLoginDTO usuarioLogin)
        {
            try
            {
                var usuario = _usuarioService.Login(usuarioLogin.Email, usuarioLogin.Senha);
                if (usuario == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(new { Token = usuario });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao realizar o login: " + ex.Message);
            }
        }

        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] UsuarioResquestDTO usuario)
        {
            try
            {
                var usuarioCadastrado = _usuarioService.Cadastrar(usuario);
                if (usuarioCadastrado == null)
                    return BadRequest("Erro ao cadastrar usuário.");

                return Ok(usuarioCadastrado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao cadastrar o usuário: " + ex.Message);
            }
        }

        [HttpGet("buscar")]
        [Authorize]
        public IActionResult BuscarUsuarioPorId()
        {
            try
            {
                var token = ObterIDDoToken();
                var userId = _jwtToken.ObterUsuarioIdDoToken(token);

                if (userId == null)
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");

                var usuario = _usuarioService.BuscarUsuarioPorId(userId.Value);
                if (usuario == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar o usuário: " + ex.Message);
            }
        }
    }
}
