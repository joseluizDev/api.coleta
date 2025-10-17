using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Services;
using api.safra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/visualizar-mapa")]
    public class VisualizarMapaController : BaseController
    {
        private readonly VisualizarMapaService _visualizarMapaService;
        private readonly INotificador _notificador;
        private readonly IJwtToken _jwtToken;

        public VisualizarMapaController(
            VisualizarMapaService visualizarMapaService,
            INotificador notificador,
            IJwtToken jwtToken)
            : base(notificador)
        {
            _visualizarMapaService = visualizarMapaService;
            _notificador = notificador;
            _jwtToken = jwtToken;
        }

        [HttpPost("salvar")]
        [Authorize]
        public IActionResult Salvar([FromBody] VisualizarMapInputDto visualizarMapa)
        {
            try
            {
                // Validação básica do modelo
                if (visualizarMapa == null)
                {
                    return BadRequest("Dados da visualização de mapa são obrigatórios.");
                }

                // Validações específicas
                if (visualizarMapa.TalhaoID == Guid.Empty)
                {
                    return BadRequest("TalhaoID é obrigatório.");
                }

                if (visualizarMapa.FuncionarioID == Guid.Empty)
                {
                    return BadRequest("FuncionarioID é obrigatório.");
                }

                if (string.IsNullOrEmpty(visualizarMapa.TipoColeta))
                {
                    return BadRequest("TipoColeta é obrigatório.");
                }

                if (visualizarMapa.TipoAnalise == null || !visualizarMapa.TipoAnalise.Any())
                {
                    return BadRequest("TipoAnalise é obrigatório.");
                }

                if (string.IsNullOrEmpty(visualizarMapa.Profundidade))
                {
                    return BadRequest("Profundidade é obrigatória.");
                }

                var token = ObterIDDoToken();
                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null)
                {
                    return BadRequest("Token inválido ou usuário não encontrado.");
                }
                Guid userId = userIdNullable.Value;

                var visualizarMapaSalvo = _visualizarMapaService.Salvar(userId, visualizarMapa);
                if (visualizarMapaSalvo == null)
                    return BadRequest("Erro ao salvar visualização de mapa. Verifique se o talhão e funcionário existem no sistema.");

                // Enviar notificação de forma assíncrona
                _ = Task.Run(() => _visualizarMapaService.EnviarNotificacaoVisualizacaoMapaAsync(visualizarMapaSalvo));

                return Ok(visualizarMapaSalvo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Erro de validação: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Erro de operação: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log detalhado do erro
                Console.WriteLine($"Erro detalhado ao salvar visualização de mapa:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    message = "Ocorreu um erro interno ao salvar a visualização de mapa.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("atualizar/{id}")]
        [Authorize]
        public IActionResult Atualizar([FromRoute] Guid id, [FromBody] VisualizarMapInputDto visualizarMapa)
        {
            try
            {
                // Validação básica do modelo
                if (visualizarMapa == null)
                {
                    return BadRequest("Dados da visualização de mapa são obrigatórios.");
                }

                // Validação do ID
                if (id == Guid.Empty)
                {
                    return BadRequest("ID da visualização de mapa é obrigatório.");
                }

                var token = ObterIDDoToken();
                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null)
                {
                    return BadRequest("Token inválido ou usuário não encontrado.");
                }
                Guid userId = userIdNullable.Value;

                var visualizarMapaAtualizado = _visualizarMapaService.Atualizar(userId, id, visualizarMapa);
                if (visualizarMapaAtualizado == null)
                    return BadRequest("Erro ao atualizar visualização de mapa. Verifique se existe e pertence ao usuário.");

                return Ok(visualizarMapaAtualizado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Erro de validação: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Erro de operação: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log detalhado do erro
                Console.WriteLine($"Erro detalhado ao atualizar visualização de mapa:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    message = "Ocorreu um erro interno ao atualizar a visualização de mapa.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] QueryVisualizarMap query)
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userIdNullable.HasValue)
            {
                var safras = _visualizarMapaService.Listar(userIdNullable.Value, query);
                return Ok(safras);

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Excluir([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var safras = _visualizarMapaService.ExcluirColeta(userId, id);
                if (safras != true)
                {
                    return BadRequest("Erro ao excluir visualização de mapa.");
                }
                return Ok("Visualização de mapa excluída com sucesso.");

            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }



        [HttpPost("diagnostico")]
        [Authorize]
        public IActionResult Diagnostico([FromBody] VisualizarMapInputDto visualizarMapa)
        {
            try
            {
                var diagnostico = new
                {
                    ReceivedData = new
                    {
                        NomeColeta = visualizarMapa.NomeColeta,
                        TipoColeta = visualizarMapa.TipoColeta,
                        TipoAnalise = visualizarMapa.TipoAnalise,
                        FuncionarioID = visualizarMapa.FuncionarioID,
                        TalhaoID = visualizarMapa.TalhaoID,
                        Observacao = visualizarMapa.Observacao,
                        Profundidade = visualizarMapa.Profundidade,
                        GeojsonValueKind = visualizarMapa.Geojson.ValueKind.ToString(),
                        GeojsonLength = visualizarMapa.Geojson.ToString().Length
                    },
                    TokenInfo = new
                    {
                        Token = ObterIDDoToken(),
                        UserId = _jwtToken.ObterUsuarioIdDoToken(ObterIDDoToken())
                    },
                    ValidationResults = new
                    {
                        IsNomeColetaValid = !string.IsNullOrEmpty(visualizarMapa.NomeColeta),
                        IsTipoColetaValid = !string.IsNullOrEmpty(visualizarMapa.TipoColeta),
                        IsTipoAnaliseValid = visualizarMapa.TipoAnalise != null && visualizarMapa.TipoAnalise.Any(),
                        IsFuncionarioIDValid = visualizarMapa.FuncionarioID != Guid.Empty,
                        IsTalhaoIDValid = visualizarMapa.TalhaoID != Guid.Empty,
                        IsProfundidadeValid = !string.IsNullOrEmpty(visualizarMapa.Profundidade),
                        IsGeojsonValid = visualizarMapa.Geojson.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
                                        visualizarMapa.Geojson.ValueKind != System.Text.Json.JsonValueKind.Null
                    }
                };

                return Ok(diagnostico);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro no diagnóstico",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult BuscarPorId([FromRoute] Guid id)
        {
            var token = ObterIDDoToken();
            Guid userId = (Guid)_jwtToken.ObterUsuarioIdDoToken(token);
            if (userId != null)
            {
                var visualizarMapa = _visualizarMapaService.BuscarVisualizarMapaPorId(userId, id);
                if (visualizarMapa != null)
                {
                    return Ok(visualizarMapa);
                }
                return NotFound(new { message = "Visualização de mapa não encontrada." });
            }
            return BadRequest(new { message = "Token inválido ou ID do usuário não encontrado." });
        }
    }
}
