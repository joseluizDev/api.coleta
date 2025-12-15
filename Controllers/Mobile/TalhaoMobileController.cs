using api.cliente.Interfaces;
using api.coleta.Controllers;
using api.talhao.Services;
using api.talhao.Models.DTOs;
using api.fazenda.models;
using api.fazenda.repositories;
using Microsoft.AspNetCore.Mvc;
using api.coleta.Data.Repository;

namespace api.coleta.Controllers.Mobile
{
    [ApiController]
    [Route("api/mobile/talhao")]
    public class TalhaoMobileController(
        TalhaoService talhaoService,
        FazendaService fazendaService,
        UsuarioRepository usuarioRepository,
        INotificador notificador,
        IJwtToken jwtToken) : BaseController(notificador)
    {
        private readonly TalhaoService _talhaoService = talhaoService;
        private readonly FazendaService _fazendaService = fazendaService;
        private readonly UsuarioRepository _usuarioRepository = usuarioRepository;
        private readonly IJwtToken _jwtToken = jwtToken;



        [HttpGet("{id}")]
        public IActionResult BuscarTalhaoPorId(Guid id)
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token)) return BadRequest("Token não fornecido.");

                try
                {
                    if (!_jwtToken.ValidarToken(token)) return BadRequest("Token inválido.");
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null) return BadRequest("ID do usuário não encontrado no token.");

                var talhao = _talhaoService.BuscarTalhaoPorId(userId.Value, id);
                if (talhao == null) return NotFound("Talhão não encontrado.");

                return Ok(talhao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar o talhão: " + ex.Message);
            }
        }

        [HttpGet("fazendas-talhoes")]
        public IActionResult ListarFazendasComTalhoes()
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token)) return BadRequest("Token não fornecido.");

                try
                {
                    if (!_jwtToken.ValidarToken(token)) return BadRequest("Token inválido.");
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null) return BadRequest("ID do usuário não encontrado no token.");

                // Buscar fazendas com seus talhões em uma única consulta otimizada
                var fazendasComTalhoes = _fazendaService.ListarFazendasComTalhoesPorUsuarioOuAdmin(userId.Value);

                return Ok(fazendasComTalhoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao buscar as fazendas com talhões: " + ex.Message);
            }
        }

        [HttpPost("salvar")]
        public IActionResult SalvarTalhao([FromBody] TalhaoMobileRequestDTO request)
        {
            try
            {
                var token = ObterIDDoToken();

                if (string.IsNullOrEmpty(token)) return BadRequest("Token não fornecido.");

                try
                {
                    if (!_jwtToken.ValidarToken(token)) return BadRequest("Token inválido.");
                }
                catch (Exception tokenEx)
                {
                    return BadRequest($"Erro ao validar token: {tokenEx.Message}");
                }

                // 1. Pegar o ID do usuário que foi passado no token
                var userId = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userId == null) return BadRequest("ID do usuário não encontrado no token.");

                // 2. Pegar o usuário para obter o ID do admin
                var usuario = _usuarioRepository.ObterPorId(userId.Value);
                if (usuario == null) return NotFound("Usuário não encontrado.");

                // 3. Determinar o ID do administrador (se usuário tem adminId, usa o admin; senão o próprio usuário é o admin)
                Guid targetAdminId = usuario.adminId ?? userId.Value;

                // 4. Pegar o usuário admin (pode ser o próprio usuário se ele for o admin)
                var usuarioAdmin = usuario.adminId.HasValue
                    ? _usuarioRepository.ObterPorId(usuario.adminId.Value)
                    : usuario;
                if (usuarioAdmin == null) return NotFound("Administrador não encontrado.");

                // 5. Buscar a fazenda para obter o ClienteID
                var fazenda = _fazendaService.BuscarFazendaPorId(targetAdminId, request.FazendaID);
                if (fazenda == null) return NotFound("Fazenda não encontrada.");

                // 6. Criar o talhão para o usuário admin
                var talhaoRequest = new TalhaoRequestDTO
                {
                    FazendaID = request.FazendaID,
                    ClienteID = fazenda.ClienteID,
                    Talhoes = new List<Talhoes>
                    {
                        new Talhoes
                        {
                            Area = request.Area,
                            Nome = request.Nome,
                            observacao = request.Observacao,
                            Coordenadas = request.Coordenadas.Select(c => new Coordenada
                            {
                                Lat = c.Lat,
                                Lng = c.Lng
                            }).ToList()
                        }
                    }
                };

                // Salvar o talhão como sendo do usuário admin
                var resultado = _talhaoService.SalvarTalhoes(usuarioAdmin.Id, talhaoRequest);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao salvar o talhão: " + ex.Message);
            }
        }
    }
}