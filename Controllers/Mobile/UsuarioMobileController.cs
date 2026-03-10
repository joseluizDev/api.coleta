using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.coleta.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/usuario")]
    public class UsuarioMobileController : BaseController
    {
        private readonly UsuarioService _usuarioService;
        private readonly IJwtToken _jwtToken;

        public UsuarioMobileController(UsuarioService usuarioService, INotificador notificador, IJwtToken jwtToken) : base(notificador)
        {
            _usuarioService = usuarioService;
            _jwtToken = jwtToken;
        }

        [HttpGet("buscar-token")]
        public IActionResult BuscarUsuarioPorToken()
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token não fornecido.");
                }

                try
                {
                    if (!_jwtToken.ValidarToken(token))
                    {
                        return BadRequest("Token inválido.");
                    }
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null)
                {
                    return BadRequest("ID do usuário não encontrado no token.");
                }

                var usuario = _usuarioService.BuscarUsuarioPorId(userId.Value);
                if (usuario == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar o usuário: " + ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UsuarioLoginDTO usuario)
        {
            try
            {
                var tokens = _usuarioService.LoginMobile(usuario);
                if (tokens == null)
                {
                    return NotFound("Usuário ou senha inválidos.");
                }
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao realizar o login: " + ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequestDTO? body)
        {
            try
            {
                var refreshToken = body?.RefreshToken ?? ObterIDDoToken();
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return BadRequest(new { errors = new[] { "Refresh token não fornecido." } });

                var tokens = _usuarioService.RefreshToken(refreshToken);
                if (tokens == null)
                    return Unauthorized(new { errors = new[] { "Refresh token inválido ou expirado." } });

                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao atualizar o token: " + ex.Message);
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequestDTO? body)
        {
            try
            {
                var refreshToken = body?.RefreshToken ?? ObterIDDoToken();
                if (!string.IsNullOrWhiteSpace(refreshToken))
                    _usuarioService.Logout(refreshToken);

                return Ok(new { message = "Logout realizado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao realizar logout: " + ex.Message);
            }
        }

        [HttpPut("atualizar")]
        public IActionResult AtualizarUsuario([FromBody] UsuarioResquestDTO usuario)
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token não fornecido.");
                }

                try
                {
                    if (!_jwtToken.ValidarToken(token))
                    {
                        return BadRequest("Token inválido.");
                    }
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null)
                {
                    return BadRequest("ID do usuário não encontrado no token.");
                }

                var usuarioAtualizado = _usuarioService.AtualizarUsuario(userId.Value, usuario);
                if (usuarioAtualizado == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                return Ok(usuarioAtualizado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao atualizar o usuário: " + ex.Message);
            }
        }
    }
}
