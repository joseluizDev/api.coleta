using api.cliente.Interfaces;
using api.cliente.Repositories;
using api.coleta.Data;
using api.coleta.Data.Repository;
using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Repositories;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de assinaturas.
    ///
    /// IMPORTANTE: Os endpoints de criacao de assinatura com pagamento (PIX, Boleto, Cartao)
    /// e planos foram migrados para o Gateway de Pagamentos Python (porta 8001).
    ///
    /// Endpoints do Gateway:
    /// - POST /api/v1/assinaturas/pix - Criar assinatura com PIX
    /// - GET /api/v1/planos - Listar planos
    /// - POST /api/v1/subscriptions/check - Verificar licenca
    /// </summary>
    [ApiController]
    [Route("api/assinatura")]
    [Authorize]
    public class AssinaturaController : BaseController
    {
        private readonly AssinaturaService _assinaturaService;
        private readonly LicenseService _licenseService;
        private readonly IGatewayService _gatewayService;
        private readonly ClienteRepository _clienteRepo;
        private readonly UsuarioRepository _usuarioRepo;
        private readonly IJwtToken _jwtToken;

        public AssinaturaController(
            AssinaturaService assinaturaService,
            LicenseService licenseService,
            IGatewayService gatewayService,
            ClienteRepository clienteRepo,
            UsuarioRepository usuarioRepo,
            IJwtToken jwtToken,
            INotificador notificador) : base(notificador)
        {
            _assinaturaService = assinaturaService;
            _licenseService = licenseService;
            _gatewayService = gatewayService;
            _clienteRepo = clienteRepo;
            _usuarioRepo = usuarioRepo;
            _jwtToken = jwtToken;
        }

        /// <summary>
        /// Verifica status de pagamento PIX de uma assinatura
        /// </summary>
        [HttpGet("verificar-pagamento/{id}")]
        public async Task<IActionResult> VerificarPagamento([FromRoute] Guid id)
        {
            var resultado = await _assinaturaService.VerificarPagamentoPixAsync(id);
            return CustomResponse(resultado);
        }

        /// <summary>
        /// Ativa assinatura manualmente (admin only)
        /// </summary>
        [HttpPost("{id}/ativar")]
        public async Task<IActionResult> AtivarAssinatura([FromRoute] Guid id, [FromBody] AtivarAssinaturaDTO? dto)
        {
            var assinatura = await _assinaturaService.AtivarAssinaturaManualAsync(id, dto?.Observacao);
            return CustomResponse(assinatura);
        }

        /// <summary>
        /// Cancela assinatura
        /// </summary>
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarAssinatura([FromRoute] Guid id)
        {
            var assinatura = await _assinaturaService.CancelarAssinaturaAsync(id);
            return CustomResponse(assinatura);
        }

        /// <summary>
        /// Lista assinaturas do cliente do usuario logado
        /// </summary>
        [HttpGet("minhas")]
        public async Task<IActionResult> MinhasAssinaturas()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();
            if (clienteId == null)
            {
                return NotFound("Cliente nao encontrado para o usuario logado.");
            }

            var assinaturas = await _assinaturaService.ListarAssinaturasDoClienteAsync(clienteId.Value);
            return Ok(assinaturas);
        }

        /// <summary>
        /// Obtem assinatura ativa do cliente do usuario logado
        /// </summary>
        [HttpGet("ativa")]
        public async Task<IActionResult> ObterAssinaturaAtiva()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();

            // Tenta buscar pelo clienteId primeiro
            if (clienteId != null)
            {
                var assinatura = await _assinaturaService.ObterAssinaturaAtivaDoClienteAsync(clienteId.Value);
                if (assinatura != null)
                {
                    return Ok(assinatura);
                }
            }

            // Fallback: busca pelo UsuarioId diretamente
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId != null)
            {
                var assinatura = await _assinaturaService.ObterAssinaturaAtivaDoUsuarioAsync(userId.Value);
                if (assinatura != null)
                {
                    return Ok(assinatura);
                }
            }

            return NotFound("Nenhuma assinatura ativa encontrada.");
        }

        /// <summary>
        /// Verifica status da licenca do cliente do usuario logado.
        /// Primeiro tenta via Gateway de Pagamentos, depois fallback para banco local.
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> VerificarStatus()
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();

            // Tentar verificar via Gateway de Pagamentos primeiro
            var gatewayResponse = await _gatewayService.VerificarLicencaAsync(userId, clienteId);
            if (gatewayResponse != null)
            {
                return Ok(new
                {
                    temLicenca = gatewayResponse.TemLicenca,
                    licencaAtiva = gatewayResponse.LicencaAtiva,
                    statusMensagem = gatewayResponse.StatusMensagem,
                    diasRestantes = gatewayResponse.DiasRestantes,
                    plano = gatewayResponse.Plano,
                    fonte = "gateway"
                });
            }

            // Fallback: verificar no banco local
            if (clienteId != null)
            {
                var statusByCliente = await _licenseService.VerificarLicencaDoClienteAsync(clienteId.Value);
                if (statusByCliente.LicencaAtiva)
                {
                    return Ok(statusByCliente);
                }
            }

            if (userId != null)
            {
                var statusByUser = await _licenseService.VerificarLicencaDoUsuarioAsync(userId.Value);
                return Ok(statusByUser);
            }

            return Ok(new
            {
                temLicenca = false,
                licencaAtiva = false,
                statusMensagem = "Nenhuma licenca encontrada. Adquira um plano para continuar.",
                alertas = new List<string> { "Sem licenca ativa" }
            });
        }

        /// <summary>
        /// Lista assinaturas de um cliente especifico (admin only)
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> ListarAssinaturasDoCliente([FromRoute] Guid clienteId)
        {
            var assinaturas = await _assinaturaService.ListarAssinaturasDoClienteAsync(clienteId);
            return Ok(assinaturas);
        }

        /// <summary>
        /// Limpa todas as assinaturas do cliente do usuario logado (desenvolvimento)
        /// </summary>
        [HttpDelete("limpar-historico")]
        public async Task<IActionResult> LimparHistorico()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();
            if (clienteId == null)
            {
                return NotFound("Cliente nao encontrado para o usuario logado.");
            }

            var deletados = await _assinaturaService.DeletarTodasAssinaturasDoClienteAsync(clienteId.Value);
            return Ok(new { message = $"{deletados} assinaturas removidas." });
        }

        /// <summary>
        /// Lista historico de pagamentos do usuario logado
        /// </summary>
        [HttpGet("historico-pagamentos")]
        public async Task<IActionResult> ListarHistoricoPagamentos()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();

            // Tenta buscar pelo clienteId primeiro
            if (clienteId != null)
            {
                var historico = await _assinaturaService.ListarHistoricoPagamentosDoClienteAsync(clienteId.Value);
                if (historico.Any())
                {
                    return Ok(historico);
                }
            }

            // Fallback: busca pelo UsuarioId diretamente
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId != null)
            {
                var historico = await _assinaturaService.ListarHistoricoPagamentosDoUsuarioAsync(userId.Value);
                return Ok(historico);
            }

            return Ok(new List<HistoricoPagamentoDTO>());
        }

        /// <summary>
        /// Lista planos disponiveis via Gateway de Pagamentos
        /// </summary>
        [HttpGet("planos")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarPlanos()
        {
            var planos = await _gatewayService.ListarPlanosAsync();
            return Ok(planos);
        }

        /// <summary>
        /// Cria assinatura com pagamento PIX via Gateway de Pagamentos.
        /// Compatibilidade com frontend existente.
        /// </summary>
        [HttpPost("criar-com-pix")]
        public async Task<IActionResult> CriarAssinaturaComPix([FromBody] CriarAssinaturaPixDTO dto)
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return Unauthorized("Usuario nao autenticado");
            }

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaPixAsync(dto.PlanoId, userId.Value, dto.ClienteId);

            if (response == null)
            {
                return BadRequest(new { message = errorMessage ?? "Erro ao criar assinatura PIX. Tente novamente." });
            }

            // Mapear resposta para formato esperado pelo frontend
            return Ok(new
            {
                assinatura = new
                {
                    id = response.Assinatura?.Id
                },
                pagamento = new
                {
                    qrCode = response.PixCopiaCola,
                    qrCodeImagem = response.PixQrCodeBase64,
                    txId = response.PagamentoId.ToString(),
                    dataExpiracao = DateTime.UtcNow.AddHours(24)
                }
            });
        }

        /// <summary>
        /// Cria assinatura com pagamento PIX vinculada ao usuario (sem cliente).
        /// Compatibilidade com frontend existente.
        /// </summary>
        [HttpPost("usuario/criar-com-pix")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComPix([FromBody] CriarAssinaturaUsuarioPixDTO dto)
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return Unauthorized("Usuario nao autenticado");
            }

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaPixAsync(dto.PlanoId, userId.Value, null);

            if (response == null)
            {
                return BadRequest(new { message = errorMessage ?? "Erro ao criar assinatura PIX. Tente novamente." });
            }

            // Mapear resposta para formato esperado pelo frontend
            return Ok(new
            {
                assinatura = new
                {
                    id = response.Assinatura?.Id
                },
                pagamento = new
                {
                    qrCode = response.PixCopiaCola,
                    qrCodeImagem = response.PixQrCodeBase64,
                    txId = response.PagamentoId.ToString(),
                    dataExpiracao = DateTime.UtcNow.AddHours(24)
                }
            });
        }

        /// <summary>
        /// Cria assinatura com pagamento Boleto via Gateway de Pagamentos.
        /// Busca nome e CPF do usuario logado automaticamente se nao fornecidos.
        /// </summary>
        [HttpPost("criar-com-boleto")]
        public async Task<IActionResult> CriarAssinaturaComBoleto([FromBody] CriarAssinaturaBoletoDTO dto)
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return Unauthorized("Usuario nao autenticado");
            }

            // Buscar dados do usuario se nao fornecidos
            var nomePagador = dto.NomePagador;
            var cpfCnpj = dto.CpfCnpj;

            if (string.IsNullOrWhiteSpace(nomePagador) || string.IsNullOrWhiteSpace(cpfCnpj))
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(userId.Value);
                if (usuario != null)
                {
                    if (string.IsNullOrWhiteSpace(nomePagador))
                        nomePagador = usuario.NomeCompleto;
                    if (string.IsNullOrWhiteSpace(cpfCnpj))
                        cpfCnpj = usuario.CPF;
                }
            }

            if (string.IsNullOrWhiteSpace(nomePagador))
            {
                return BadRequest(new { message = "Nome do pagador e obrigatorio para boleto. Atualize seu perfil." });
            }

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { message = "CPF/CNPJ e obrigatorio para boleto. Atualize seu perfil." });
            }

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaBoletoAsync(
                dto.PlanoId, userId.Value, nomePagador, cpfCnpj, dto.ClienteId);

            if (response == null)
            {
                return BadRequest(new { message = errorMessage ?? "Erro ao criar assinatura com boleto. Tente novamente." });
            }

            // Mapear resposta para formato esperado pelo frontend
            return Ok(new
            {
                assinatura = new
                {
                    id = response.Assinatura?.Id
                },
                pagamento = new
                {
                    codigoBarras = response.BoletoCodigoBarras,
                    url = response.BoletoUrl,
                    txId = response.PagamentoId.ToString(),
                    dataVencimento = DateTime.UtcNow.AddDays(3)
                }
            });
        }

        /// <summary>
        /// Cria assinatura com pagamento Boleto vinculada ao usuario (sem cliente).
        /// Busca nome e CPF do usuario logado automaticamente se nao fornecidos.
        /// </summary>
        [HttpPost("usuario/criar-com-boleto")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComBoleto([FromBody] CriarAssinaturaUsuarioBoletoDTO dto)
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return Unauthorized("Usuario nao autenticado");
            }

            // Buscar dados do usuario se nao fornecidos
            var nomePagador = dto.NomePagador;
            var cpfCnpj = dto.CpfCnpj;

            if (string.IsNullOrWhiteSpace(nomePagador) || string.IsNullOrWhiteSpace(cpfCnpj))
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(userId.Value);
                if (usuario != null)
                {
                    if (string.IsNullOrWhiteSpace(nomePagador))
                        nomePagador = usuario.NomeCompleto;
                    if (string.IsNullOrWhiteSpace(cpfCnpj))
                        cpfCnpj = usuario.CPF;
                }
            }

            if (string.IsNullOrWhiteSpace(nomePagador))
            {
                return BadRequest(new { message = "Nome do pagador e obrigatorio para boleto. Atualize seu perfil." });
            }

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { message = "CPF/CNPJ e obrigatorio para boleto. Atualize seu perfil." });
            }

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaBoletoAsync(
                dto.PlanoId, userId.Value, nomePagador, cpfCnpj, null);

            if (response == null)
            {
                return BadRequest(new { message = errorMessage ?? "Erro ao criar assinatura com boleto. Tente novamente." });
            }

            // Mapear resposta para formato esperado pelo frontend
            return Ok(new
            {
                assinatura = new
                {
                    id = response.Assinatura?.Id
                },
                pagamento = new
                {
                    codigoBarras = response.BoletoCodigoBarras,
                    url = response.BoletoUrl,
                    txId = response.PagamentoId.ToString(),
                    dataVencimento = DateTime.UtcNow.AddDays(3)
                }
            });
        }

        /// <summary>
        /// Cria assinatura com Pix Automatico (recorrencia) via Gateway de Pagamentos.
        /// O Pix Automatico permite que o cliente autorize uma unica vez e as cobrancas
        /// futuras sejam feitas automaticamente.
        /// </summary>
        [HttpPost("criar-com-pix-automatico")]
        public async Task<IActionResult> CriarAssinaturaComPixAutomatico([FromBody] CriarAssinaturaPixAutomaticoDTO dto)
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return Unauthorized("Usuario nao autenticado");
            }

            // Buscar dados do usuario se nao fornecidos
            var nomeDevedor = dto.NomeDevedor;
            var cpfCnpj = dto.CpfCnpj;

            if (string.IsNullOrWhiteSpace(nomeDevedor) || string.IsNullOrWhiteSpace(cpfCnpj))
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(userId.Value);
                if (usuario != null)
                {
                    if (string.IsNullOrWhiteSpace(nomeDevedor))
                        nomeDevedor = usuario.NomeCompleto;
                    if (string.IsNullOrWhiteSpace(cpfCnpj))
                        cpfCnpj = usuario.CPF;
                }
            }

            if (string.IsNullOrWhiteSpace(nomeDevedor))
            {
                return BadRequest(new { message = "Nome do devedor e obrigatorio para Pix Automatico. Atualize seu perfil." });
            }

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { message = "CPF/CNPJ e obrigatorio para Pix Automatico. Atualize seu perfil." });
            }

            var periodicidade = string.IsNullOrWhiteSpace(dto.Periodicidade) ? "ANUAL" : dto.Periodicidade;

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaPixAutomaticoAsync(
                dto.PlanoId, userId.Value, nomeDevedor, cpfCnpj, periodicidade, dto.ClienteId);

            if (response == null)
            {
                return BadRequest(new { message = errorMessage ?? "Erro ao criar assinatura com Pix Automatico. Tente novamente." });
            }

            // Mapear resposta para formato esperado pelo frontend
            return Ok(new
            {
                assinatura = new
                {
                    id = response.Assinatura?.Id
                },
                recorrencia = new
                {
                    idRec = response.IdRec,
                    contrato = response.Contrato,
                    statusRecorrencia = response.StatusRecorrencia,
                    qrCode = response.PixCopiaCola,
                    qrCodeImagem = response.PixQrCodeBase64,
                    linkAutorizacao = response.LinkAutorizacao
                },
                pagamento = new
                {
                    qrCode = response.PixCopiaCola,
                    qrCodeImagem = response.PixQrCodeBase64,
                    txId = response.PagamentoId.ToString()
                }
            });
        }

        /// <summary>
        /// Cria assinatura com Pix Automatico vinculada ao usuario (sem cliente).
        /// </summary>
        [HttpPost("usuario/criar-com-pix-automatico")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComPixAutomatico([FromBody] CriarAssinaturaUsuarioPixAutomaticoDTO dto)
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return Unauthorized("Usuario nao autenticado");
            }

            // Buscar dados do usuario se nao fornecidos
            var nomeDevedor = dto.NomeDevedor;
            var cpfCnpj = dto.CpfCnpj;

            if (string.IsNullOrWhiteSpace(nomeDevedor) || string.IsNullOrWhiteSpace(cpfCnpj))
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(userId.Value);
                if (usuario != null)
                {
                    if (string.IsNullOrWhiteSpace(nomeDevedor))
                        nomeDevedor = usuario.NomeCompleto;
                    if (string.IsNullOrWhiteSpace(cpfCnpj))
                        cpfCnpj = usuario.CPF;
                }
            }

            if (string.IsNullOrWhiteSpace(nomeDevedor))
            {
                return BadRequest(new { message = "Nome do devedor e obrigatorio para Pix Automatico. Atualize seu perfil." });
            }

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { message = "CPF/CNPJ e obrigatorio para Pix Automatico. Atualize seu perfil." });
            }

            var periodicidade = string.IsNullOrWhiteSpace(dto.Periodicidade) ? "ANUAL" : dto.Periodicidade;

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaPixAutomaticoAsync(
                dto.PlanoId, userId.Value, nomeDevedor, cpfCnpj, periodicidade, null);

            if (response == null)
            {
                return BadRequest(new { message = errorMessage ?? "Erro ao criar assinatura com Pix Automatico. Tente novamente." });
            }

            // Mapear resposta para formato esperado pelo frontend
            return Ok(new
            {
                assinatura = new
                {
                    id = response.Assinatura?.Id
                },
                recorrencia = new
                {
                    idRec = response.IdRec,
                    contrato = response.Contrato,
                    statusRecorrencia = response.StatusRecorrencia,
                    qrCode = response.PixCopiaCola,
                    qrCodeImagem = response.PixQrCodeBase64,
                    linkAutorizacao = response.LinkAutorizacao
                },
                pagamento = new
                {
                    qrCode = response.PixCopiaCola,
                    qrCodeImagem = response.PixQrCodeBase64,
                    txId = response.PagamentoId.ToString()
                }
            });
        }

        private async Task<Guid?> ObterClienteIdDoUsuarioLogadoAsync()
        {
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId == null)
            {
                return null;
            }

            // Find cliente by UsuarioID
            var clientes = _clienteRepo.ListarTodosClientes(userId.Value);
            var cliente = clientes.FirstOrDefault();

            return cliente?.Id;
        }
    }

    public class AtivarAssinaturaDTO
    {
        public string? Observacao { get; set; }
    }

    public class CriarAssinaturaPixDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }
    }

    public class CriarAssinaturaUsuarioPixDTO
    {
        public Guid PlanoId { get; set; }
    }

    public class CriarAssinaturaBoletoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }
        public string NomePagador { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
    }

    public class CriarAssinaturaUsuarioBoletoDTO
    {
        public Guid PlanoId { get; set; }
        public string NomePagador { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
    }

    public class CriarAssinaturaPixAutomaticoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid? ClienteId { get; set; }
        public string NomeDevedor { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Periodicidade { get; set; } = "ANUAL";
    }

    public class CriarAssinaturaUsuarioPixAutomaticoDTO
    {
        public Guid PlanoId { get; set; }
        public string NomeDevedor { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string Periodicidade { get; set; } = "ANUAL";
    }
}
