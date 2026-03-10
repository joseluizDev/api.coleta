using api.cliente.Interfaces;
using api.coleta.Models.DTOs.Licenciamento;
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
        private readonly IJwtToken _jwtToken;

        public AssinaturaController(
            AssinaturaService assinaturaService,
            LicenseService licenseService,
            IGatewayService gatewayService,
            IJwtToken jwtToken,
            INotificador notificador) : base(notificador)
        {
            _assinaturaService = assinaturaService;
            _licenseService = licenseService;
            _gatewayService = gatewayService;
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
            var clienteId = ObterClienteIdDoUsuarioLogado();
            if (clienteId == null)
                return NotFound("Cliente nao encontrado para o usuario logado.");

            var assinaturas = await _assinaturaService.ListarAssinaturasDoClienteAsync(clienteId.Value);
            return Ok(assinaturas);
        }

        /// <summary>
        /// Obtem assinatura ativa do cliente do usuario logado
        /// </summary>
        [HttpGet("ativa")]
        public async Task<IActionResult> ObterAssinaturaAtiva()
        {
            var (userId, clienteId) = ObterIdsDoUsuarioLogado();

            var assinatura = await _assinaturaService.ObterAssinaturaAtivaComFallbackAsync(clienteId, userId);
            if (assinatura == null)
                return NotFound("Nenhuma assinatura ativa encontrada.");

            return Ok(assinatura);
        }

        /// <summary>
        /// Verifica status da licenca do cliente do usuario logado.
        /// Primeiro tenta via Gateway de Pagamentos, depois fallback para banco local.
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> VerificarStatus()
        {
            var (userId, clienteId) = ObterIdsDoUsuarioLogado();
            var status = await _assinaturaService.VerificarStatusLicencaAsync(userId, clienteId, _licenseService);
            return Ok(status);
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
            var clienteId = ObterClienteIdDoUsuarioLogado();
            if (clienteId == null)
                return NotFound("Cliente nao encontrado para o usuario logado.");

            var deletados = await _assinaturaService.DeletarTodasAssinaturasDoClienteAsync(clienteId.Value);
            return Ok(new { message = $"{deletados} assinaturas removidas." });
        }

        /// <summary>
        /// Lista historico de pagamentos do usuario logado.
        /// Busca diretamente do Gateway de Pagamentos (fonte da verdade).
        /// </summary>
        [HttpGet("historico-pagamentos")]
        public async Task<IActionResult> ListarHistoricoPagamentos()
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var historico = await _assinaturaService.ListarHistoricoPagamentosViaGatewayAsync(userId.Value);
            return Ok(historico);
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
        /// Obtem opcoes de parcelamento para cartao de credito.
        /// Calcula parcelas com e sem juros baseado no valor e bandeira.
        /// </summary>
        [HttpGet("parcelas")]
        [AllowAnonymous]
        public IActionResult ObterParcelas([FromQuery] string bandeira, [FromQuery] int valor)
        {
            var resultado = _assinaturaService.CalcularParcelas(bandeira, valor);
            return Ok(resultado);
        }

        /// <summary>
        /// Cria assinatura com pagamento PIX via Gateway de Pagamentos.
        /// </summary>
        [HttpPost("criar-com-pix")]
        public async Task<IActionResult> CriarAssinaturaComPix([FromBody] CriarAssinaturaPixDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComPixAsync(dto.PlanoId, userId.Value, dto.ClienteId);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com pagamento PIX vinculada ao usuario (sem cliente).
        /// </summary>
        [HttpPost("usuario/criar-com-pix")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComPix([FromBody] CriarAssinaturaUsuarioPixDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComPixAsync(dto.PlanoId, userId.Value, null);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com pagamento Boleto via Gateway de Pagamentos.
        /// </summary>
        [HttpPost("criar-com-boleto")]
        public async Task<IActionResult> CriarAssinaturaComBoleto([FromBody] CriarAssinaturaBoletoDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComBoletoAsync(
                dto.PlanoId, userId.Value, dto.NomePagador, dto.CpfCnpj, dto.ClienteId);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com pagamento Boleto vinculada ao usuario (sem cliente).
        /// </summary>
        [HttpPost("usuario/criar-com-boleto")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComBoleto([FromBody] CriarAssinaturaUsuarioBoletoDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComBoletoAsync(
                dto.PlanoId, userId.Value, dto.NomePagador, dto.CpfCnpj, null);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com Pix Automatico (recorrencia) via Gateway de Pagamentos.
        /// </summary>
        [HttpPost("criar-com-pix-automatico")]
        public async Task<IActionResult> CriarAssinaturaComPixAutomatico([FromBody] CriarAssinaturaPixAutomaticoDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComPixAutomaticoAsync(
                dto.PlanoId, userId.Value, dto.NomeDevedor, dto.CpfCnpj, dto.Periodicidade, dto.ClienteId);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com Pix Automatico vinculada ao usuario (sem cliente).
        /// </summary>
        [HttpPost("usuario/criar-com-pix-automatico")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComPixAutomatico([FromBody] CriarAssinaturaUsuarioPixAutomaticoDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComPixAutomaticoAsync(
                dto.PlanoId, userId.Value, dto.NomeDevedor, dto.CpfCnpj, dto.Periodicidade, null);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com pagamento Cartao de Credito via Gateway de Pagamentos.
        /// Suporta payment_token (recomendado) e dados do cartao (legado).
        /// </summary>
        [HttpPost("criar-com-cartao")]
        public async Task<IActionResult> CriarAssinaturaComCartao([FromBody] CriarAssinaturaCartaoDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            var resultado = await _assinaturaService.CriarAssinaturaComCartaoAsync(dto.PlanoId, userId.Value, dto, dto.ClienteId);
            return ResultadoAssinatura(resultado);
        }

        /// <summary>
        /// Cria assinatura com pagamento Cartao de Credito vinculada ao usuario (sem cliente).
        /// Suporta payment_token (recomendado) e dados do cartao (legado).
        /// </summary>
        [HttpPost("usuario/criar-com-cartao")]
        public async Task<IActionResult> CriarAssinaturaUsuarioComCartao([FromBody] CriarAssinaturaUsuarioCartaoDTO dto)
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null)
                return Unauthorized("Usuario nao autenticado");

            // Mapear para o DTO base de cartao
            var cartaoDto = new CriarAssinaturaCartaoDTO
            {
                PlanoId = dto.PlanoId,
                PaymentToken = dto.PaymentToken,
                NomePagador = dto.NomePagador,
                CpfCnpj = dto.CpfCnpj,
                Email = dto.Email,
                Telefone = dto.Telefone,
                NumeroCartao = dto.NumeroCartao,
                Cvv = dto.Cvv,
                MesValidade = dto.MesValidade,
                AnoValidade = dto.AnoValidade,
                NomeCartao = dto.NomeCartao,
                Parcelas = dto.Parcelas,
                Bandeira = dto.Bandeira
            };

            var resultado = await _assinaturaService.CriarAssinaturaComCartaoAsync(dto.PlanoId, userId.Value, cartaoDto, null);
            return ResultadoAssinatura(resultado);
        }

        #region Helpers

        private Guid? ObterUsuarioIdDoToken()
        {
            var token = ObterIDDoToken();
            return _jwtToken.ObterUsuarioIdDoToken(token);
        }

        private Guid? ObterClienteIdDoUsuarioLogado()
        {
            var userId = ObterUsuarioIdDoToken();
            if (userId == null) return null;
            return _assinaturaService.ObterClienteIdDoUsuario(userId.Value);
        }

        private (Guid? userId, Guid? clienteId) ObterIdsDoUsuarioLogado()
        {
            var userId = ObterUsuarioIdDoToken();
            Guid? clienteId = null;
            if (userId != null)
                clienteId = _assinaturaService.ObterClienteIdDoUsuario(userId.Value);
            return (userId, clienteId);
        }

        private IActionResult ResultadoAssinatura<T>(AssinaturaResultDTO<T> resultado)
        {
            if (!resultado.Sucesso)
                return BadRequest(new { message = resultado.Erro });

            return Ok(resultado.Dados);
        }

        #endregion
    }
}
