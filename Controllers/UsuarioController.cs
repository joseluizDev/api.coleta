﻿using api.coleta.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using whtsapp.Controllers;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly INotificador _notificador;
        public UsuarioController(UsuarioService usuarioService, INotificador notificador)
        {
            _usuarioService = usuarioService;
            _notificador = notificador;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login(string login, string senha)
        {
            var usuario = _usuarioService.Login(login, senha);
            if (usuario == null)
                return NotFound("Usuário não encontrado");
            return Ok(usuario);
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
        [Route("atualziar")]
        public IActionResult Atualizar([FromBody] UsuarioResquestDTO usuario)
        {
            var usuarioAtualizado = _usuarioService.Atualizar(usuario);
            if (usuarioAtualizado == null)
                return BadRequest("Erro ao atualizar usuário");
            return Ok(usuarioAtualizado);
        }
    }
}
