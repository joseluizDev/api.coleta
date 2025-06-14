﻿using api.coleta.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using api.cliente.Interfaces;
using Microsoft.AspNetCore.Authorization;
using api.safra.Services;
using api.funcionario.Models.DTOs;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/usuario")]
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

        [HttpPost("cadastrar/funcionario")]
        public IActionResult Cadastrar([FromBody] UsuarioResquestDTO usuario)
        {
            try
            {
                var token = ObterIDDoToken();
                Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
                if (userId != null)
                {
                    bool cadastrado = _usuarioService.Cadastrar(userId, usuario);
                    if (!cadastrado)
                        return BadRequest("Erro ao cadastrar usuário.");

                    return Ok(new { message = "Usuário cadastrado com sucesso." });
                }
                return BadRequest("Token inválido ou ID do usuário não encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { message = ex.Message });
            }
        }

        [HttpGet("buscar")]
        [Authorize]
        public IActionResult BuscarUsuarioPorId()
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token não fornecido.");
                }

                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null)
                {
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");
                }

                Guid userId = userIdNullable.Value;

                var usuario = _usuarioService.BuscarUsuarioPorId(userId);
                if (usuario == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar o usuário: " + ex.Message);
            }
        }

        [HttpPut("atualizar")]
        [Authorize]
        public IActionResult AtualizarUsuario([FromBody] UsuarioResquestDTO usuario)
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token não fornecido.");
                }

                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null)
                {
                    return BadRequest("Token inválido ou ID do usuário não encontrado.");
                }

                Guid userId = userIdNullable.Value;

                var usuarioAtualizado = _usuarioService.AtualizarUsuario(userId, usuario);

                if (usuarioAtualizado == null)
                    return NotFound("Usuário não encontrado.");

                return Ok(usuarioAtualizado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao atualizar o usuário: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("funcionario/listar")]
        public IActionResult ListarFuncionario()
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safras = _usuarioService.ListarUsuarioFuncionario(userId);
                return Ok(safras);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("funcionario")]
        public IActionResult Funcionarios([FromQuery] QueryFuncionario query)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safras = _usuarioService.Funcionarios(query, userId);
                return Ok(safras);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpDelete]
        [Route("funcionario/deletar/{id}")]
        public IActionResult ListarFuncionario([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safras = _usuarioService.DeletarFuncionario(id, userId);
                return Ok(safras);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpGet]
        [Route("funcionario/buscar/{id}")]
        public IActionResult BuscarFuncionarioPorId([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var funcionario = _usuarioService.BuscarFuncionarioPorId(id, userId);
                if (funcionario == null)
                {
                    return NotFound(new { message = "Funcionário não encontrado." });
                }
                return Ok(funcionario);
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpPut]
        [Route("funcionario/atualizar")]
        public IActionResult AtualizarFuncionario([FromBody] FuncionarioRequestDTO funcionario)
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userIdNullable == null)
            {
                return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
            }
            Guid userId = userIdNullable.Value;
            try
            {
                var funcionarioAtualizado = _usuarioService.AtualizarFuncionario(userId, funcionario);
                if (funcionarioAtualizado == null)
                    return NotFound(new { message = "Funcionário não encontrado." });
                return Ok(funcionarioAtualizado);
            }
            catch (Exception ex)
            {
                return StatusCode(409, new { message = ex.Message });
            }
        }

    }
}
