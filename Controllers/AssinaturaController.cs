using api.cliente.Interfaces;
using api.cliente.Repositories;
using api.coleta.Data;
using api.coleta.Data.Repository;
using api.coleta.Models.DTOs.EfiPay;
using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Repositories;
using api.coleta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/assinatura")]
    [Authorize]
    public class AssinaturaController : BaseController
    {
        private readonly AssinaturaService _assinaturaService;
        private readonly LicenseService _licenseService;
        private readonly ClienteRepository _clienteRepo;
        private readonly UsuarioRepository _usuarioRepo;
        private readonly PlanoRepository _planoRepo;
        private readonly IJwtToken _jwtToken;
        private readonly IEfiPayBoletoService _boletoService;
        private readonly IEfiPayCartaoService _cartaoService;
        private readonly IEfiPayAssinaturaService _recorrenciaService;
        private readonly IUnitOfWork _unitOfWork;

        public AssinaturaController(
            AssinaturaService assinaturaService,
            LicenseService licenseService,
            ClienteRepository clienteRepo,
            UsuarioRepository usuarioRepo,
            PlanoRepository planoRepo,
            IJwtToken jwtToken,
            IEfiPayBoletoService boletoService,
            IEfiPayCartaoService cartaoService,
            IEfiPayAssinaturaService recorrenciaService,
            IUnitOfWork unitOfWork,
            INotificador notificador) : base(notificador)
        {
            _assinaturaService = assinaturaService;
            _licenseService = licenseService;
            _clienteRepo = clienteRepo;
            _usuarioRepo = usuarioRepo;
            _planoRepo = planoRepo;
            _jwtToken = jwtToken;
            _boletoService = boletoService;
            _cartaoService = cartaoService;
            _recorrenciaService = recorrenciaService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Cria assinatura com pagamento PIX
        /// </summary>
        [HttpPost("criar-com-pix")]
        public async Task<IActionResult> CriarAssinaturaComPix([FromBody] AssinaturaCreateComPixDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Se CPF não foi enviado, buscar do usuário logado
            if (string.IsNullOrWhiteSpace(dto.CpfCnpj))
            {
                var token = ObterIDDoToken();
                var userId = _jwtToken.ObterUsuarioIdDoToken(token);

                if (userId != null)
                {
                    var usuario = await _usuarioRepo.ObterPorIdAsync(userId.Value);
                    if (usuario != null && !string.IsNullOrWhiteSpace(usuario.CPF))
                    {
                        dto.CpfCnpj = usuario.CPF;
                    }
                }
            }

            // Validar se temos CPF/CNPJ
            if (string.IsNullOrWhiteSpace(dto.CpfCnpj))
            {
                return BadRequest(new { errors = new { CpfCnpj = new[] { "CPF ou CNPJ é obrigatório. Atualize seu perfil com um CPF válido." } } });
            }

            var resultado = await _assinaturaService.CriarAssinaturaComPagamentoPixAsync(dto);
            return CustomResponse(resultado);
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
        /// Cria assinatura com pagamento via Boleto
        /// </summary>
        [HttpPost("criar-com-boleto")]
        public async Task<IActionResult> CriarAssinaturaComBoleto([FromBody] AssinaturaCreateComBoletoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar dados do usuário/cliente
            var (cpfCnpj, nomeCliente, email, telefone) = await ObterDadosClienteAsync(dto.CpfCnpj, dto.Email);

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { errors = new { CpfCnpj = new[] { "CPF ou CNPJ é obrigatório." } } });
            }

            // Buscar plano
            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null)
            {
                return NotFound("Plano não encontrado.");
            }

            try
            {
                var boleto = await _boletoService.CriarBoletoAsync(
                    plano.ValorAnual,
                    cpfCnpj,
                    nomeCliente ?? "Cliente",
                    email ?? "cliente@email.com",
                    telefone,
                    dto.DataVencimento,
                    $"AgroSyste - {plano.Nome}"
                );

                // Criar assinatura pendente
                var assinatura = await _assinaturaService.CriarAssinaturaPendenteAsync(dto.PlanoId, dto.ClienteId, "boleto", boleto.ChargeId.ToString());

                return Ok(new
                {
                    assinatura,
                    boleto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cria assinatura com pagamento via Cartão de Crédito
        /// </summary>
        [HttpPost("criar-com-cartao")]
        public async Task<IActionResult> CriarAssinaturaComCartao([FromBody] AssinaturaCreateComCartaoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(dto.PaymentToken))
            {
                return BadRequest(new { errors = new { PaymentToken = new[] { "Token de pagamento é obrigatório." } } });
            }

            // Buscar dados do usuário/cliente
            var (cpfCnpj, nomeCliente, email, telefone) = await ObterDadosClienteAsync(dto.CpfCnpj, dto.Email);

            // Usar telefone do DTO se fornecido
            if (!string.IsNullOrWhiteSpace(dto.Telefone))
            {
                telefone = dto.Telefone;
            }

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { errors = new { CpfCnpj = new[] { "CPF ou CNPJ é obrigatório." } } });
            }

            if (string.IsNullOrWhiteSpace(telefone))
            {
                return BadRequest(new { errors = new { Telefone = new[] { "Telefone é obrigatório para pagamento com cartão." } } });
            }

            // Buscar plano
            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null)
            {
                return NotFound("Plano não encontrado.");
            }

            try
            {
                var cartao = await _cartaoService.CriarCobrancaCartaoAsync(
                    plano.ValorAnual,
                    dto.PaymentToken,
                    dto.Parcelas,
                    cpfCnpj,
                    nomeCliente ?? "Cliente",
                    email ?? "cliente@email.com",
                    telefone,
                    $"AgroSyste - {plano.Nome}"
                );

                if (!cartao.Aprovado)
                {
                    return BadRequest(new
                    {
                        error = "Pagamento recusado",
                        motivo = cartao.MotivoRecusa,
                        podeRetentar = cartao.PodeRetentar
                    });
                }

                // Criar assinatura ativa (pagamento aprovado)
                var assinatura = await _assinaturaService.CriarAssinaturaAtivaPorCartaoAsync(
                    dto.PlanoId,
                    dto.ClienteId,
                    cartao.ChargeId.ToString(),
                    cartao.Parcelas,
                    cartao.ValorParcela,
                    cartao.Bandeira
                );

                return Ok(new
                {
                    assinatura,
                    pagamento = cartao
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cria assinatura recorrente (mensal)
        /// </summary>
        [HttpPost("criar-recorrente")]
        public async Task<IActionResult> CriarAssinaturaRecorrente([FromBody] AssinaturaCreateRecorrenteDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar dados do usuário/cliente
            var (cpfCnpj, nomeCliente, email, telefone) = await ObterDadosClienteAsync(dto.CpfCnpj, dto.Email);

            if (string.IsNullOrWhiteSpace(cpfCnpj))
            {
                return BadRequest(new { errors = new { CpfCnpj = new[] { "CPF ou CNPJ é obrigatório." } } });
            }

            // Buscar plano
            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null)
            {
                return NotFound("Plano não encontrado.");
            }

            // Verificar se plano tem ID na EfiPay
            if (!plano.EfiPayPlanIdInt.HasValue)
            {
                return BadRequest(new { error = "Plano não configurado para assinaturas recorrentes." });
            }

            try
            {
                AssinaturaRecorrenteDTO recorrencia;

                if (dto.MetodoPagamento == "credit_card")
                {
                    if (string.IsNullOrEmpty(dto.PaymentToken))
                    {
                        return BadRequest(new { errors = new { PaymentToken = new[] { "Token de pagamento é obrigatório para cartão." } } });
                    }

                    recorrencia = await _recorrenciaService.CriarAssinaturaRecorrenteCartaoAsync(
                        plano.EfiPayPlanIdInt.Value,
                        plano.ValorMensal,
                        dto.PaymentToken,
                        cpfCnpj,
                        nomeCliente ?? "Cliente",
                        email ?? "cliente@email.com",
                        telefone
                    );
                }
                else // banking_billet
                {
                    recorrencia = await _recorrenciaService.CriarAssinaturaRecorrenteBoletoAsync(
                        plano.EfiPayPlanIdInt.Value,
                        plano.ValorMensal,
                        cpfCnpj,
                        nomeCliente ?? "Cliente",
                        email ?? "cliente@email.com",
                        telefone
                    );
                }

                // Criar assinatura no sistema
                var assinatura = await _assinaturaService.CriarAssinaturaRecorrenteAsync(
                    dto.PlanoId,
                    dto.ClienteId,
                    recorrencia.SubscriptionId.ToString(),
                    dto.MetodoPagamento
                );

                return Ok(new
                {
                    assinatura,
                    recorrencia
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém opções de parcelamento para cartão
        /// </summary>
        [HttpGet("parcelas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterParcelas([FromQuery] string bandeira, [FromQuery] decimal valor)
        {
            if (string.IsNullOrEmpty(bandeira))
            {
                return BadRequest(new { error = "Bandeira do cartão é obrigatória." });
            }

            if (valor <= 0)
            {
                return BadRequest(new { error = "Valor deve ser maior que zero." });
            }

            try
            {
                var parcelas = await _cartaoService.ObterParcelasAsync(bandeira, valor);
                return Ok(parcelas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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
        /// Lista assinaturas do cliente do usuário logado
        /// </summary>
        [HttpGet("minhas")]
        public async Task<IActionResult> MinhasAssinaturas()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();
            if (clienteId == null)
            {
                return NotFound("Cliente não encontrado para o usuário logado.");
            }

            var assinaturas = await _assinaturaService.ListarAssinaturasDoClienteAsync(clienteId.Value);
            return Ok(assinaturas);
        }

        /// <summary>
        /// Obtém assinatura ativa do cliente do usuário logado
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
        /// Verifica status da licença do cliente do usuário logado
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> VerificarStatus()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();

            // Se encontrou cliente, busca licença pelo clienteId
            if (clienteId != null)
            {
                var statusByCliente = await _licenseService.VerificarLicencaDoClienteAsync(clienteId.Value);

                // Se encontrou licença ativa, retorna
                if (statusByCliente.LicencaAtiva)
                {
                    return Ok(statusByCliente);
                }
            }

            // Fallback: busca diretamente pelo UsuarioId (caso cliente tenha assinatura mas UsuarioID diverge)
            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId != null)
            {
                var statusByUser = await _licenseService.VerificarLicencaDoUsuarioAsync(userId.Value);
                return Ok(statusByUser);
            }

            // Se não encontrou por nenhum método
            return Ok(new
            {
                temLicenca = false,
                licencaAtiva = false,
                statusMensagem = "Nenhuma licenca encontrada. Adquira um plano para continuar.",
                alertas = new List<string> { "Sem licenca ativa" }
            });
        }

        /// <summary>
        /// Lista assinaturas de um cliente específico (admin only)
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> ListarAssinaturasDoCliente([FromRoute] Guid clienteId)
        {
            var assinaturas = await _assinaturaService.ListarAssinaturasDoClienteAsync(clienteId);
            return Ok(assinaturas);
        }

        /// <summary>
        /// Limpa todas as assinaturas do cliente do usuário logado (desenvolvimento)
        /// </summary>
        [HttpDelete("limpar-historico")]
        public async Task<IActionResult> LimparHistorico()
        {
            var clienteId = await ObterClienteIdDoUsuarioLogadoAsync();
            if (clienteId == null)
            {
                return NotFound("Cliente não encontrado para o usuário logado.");
            }

            var deletados = await _assinaturaService.DeletarTodasAssinaturasDoClienteAsync(clienteId.Value);
            return Ok(new { message = $"{deletados} assinaturas removidas." });
        }

        /// <summary>
        /// Lista histórico de pagamentos do usuário logado
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

        /// <summary>
        /// Registra um plano local na EfiPay e salva o ID retornado
        /// </summary>
        [HttpPost("registrar-plano-efipay/{planoId}")]
        public async Task<IActionResult> RegistrarPlanoEfiPay([FromRoute] Guid planoId)
        {
            var plano = await _planoRepo.ObterPorIdAsync(planoId);
            if (plano == null)
            {
                return NotFound("Plano não encontrado.");
            }

            if (plano.EfiPayPlanIdInt.HasValue)
            {
                return Ok(new {
                    message = "Plano já registrado na EfiPay",
                    efiPayPlanId = plano.EfiPayPlanIdInt.Value
                });
            }

            try
            {
                // Criar plano na EfiPay (mensal, 12 repetições = 1 ano)
                var efiPayPlan = await _recorrenciaService.CriarPlanoEfiPayAsync(
                    $"AgroSyste - {plano.Nome}",
                    intervaloMeses: 1,
                    repeticoes: 12
                );

                // Salvar ID no plano local
                plano.EfiPayPlanIdInt = efiPayPlan.PlanId;
                _planoRepo.Atualizar(plano);
                _unitOfWork.Commit();

                return Ok(new
                {
                    message = "Plano registrado na EfiPay com sucesso",
                    efiPayPlanId = efiPayPlan.PlanId,
                    efiPayPlanName = efiPayPlan.Name
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task<(string? CpfCnpj, string? Nome, string? Email, string? Telefone)> ObterDadosClienteAsync(string? cpfCnpjDto, string? emailDto)
        {
            string? cpfCnpj = cpfCnpjDto;
            string? nome = null;
            string? email = emailDto;
            string? telefone = null;

            var token = ObterIDDoToken();
            var userId = _jwtToken.ObterUsuarioIdDoToken(token);

            if (userId != null)
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(userId.Value);
                if (usuario != null)
                {
                    if (string.IsNullOrWhiteSpace(cpfCnpj) && !string.IsNullOrWhiteSpace(usuario.CPF))
                    {
                        cpfCnpj = usuario.CPF;
                    }
                    nome = usuario.NomeCompleto;
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        email = usuario.Email;
                    }
                    telefone = usuario.Telefone;
                }
            }

            return (cpfCnpj, nome, email, telefone);
        }
    }

    public class AtivarAssinaturaDTO
    {
        public string? Observacao { get; set; }
    }

    public class AssinaturaCreateComBoletoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid ClienteId { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public DateTime? DataVencimento { get; set; }
    }

    public class AssinaturaCreateComCartaoDTO
    {
        public Guid PlanoId { get; set; }
        public Guid ClienteId { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string PaymentToken { get; set; } = string.Empty;
        public int Parcelas { get; set; } = 1;
    }
}
