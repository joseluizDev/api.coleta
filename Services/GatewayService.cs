using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using api.coleta.Settings;
using Microsoft.Extensions.Options;

namespace api.coleta.Services
{
    public interface IGatewayService
    {
        Task<GatewayLicenseResponse?> VerificarLicencaAsync(Guid? usuarioId = null, Guid? clienteId = null);
        Task<GatewayPlanoResponse[]> ListarPlanosAsync();
        Task<(GatewayAssinaturaPixResponse? Response, string? ErrorMessage)> CriarAssinaturaPixAsync(Guid planoId, Guid usuarioId, Guid? clienteId = null);
        Task<(GatewayAssinaturaBoletoResponse? Response, string? ErrorMessage)> CriarAssinaturaBoletoAsync(Guid planoId, Guid usuarioId, string nomePagador, string cpfCnpj, Guid? clienteId = null);
        Task<(GatewayAssinaturaPixAutomaticoResponse? Response, string? ErrorMessage)> CriarAssinaturaPixAutomaticoAsync(Guid planoId, Guid usuarioId, string nomeDevedor, string cpfCnpj, string periodicidade = "ANUAL", Guid? clienteId = null);
        Task<(GatewayAssinaturaCartaoResponse? Response, string? ErrorMessage)> CriarAssinaturaCartaoAsync(Guid planoId, Guid usuarioId, string paymentToken, int parcelas = 1, string bandeira = "", Guid? clienteId = null);
        Task<GatewayVerificacaoPagamentoResponse?> VerificarPagamentoAsync(Guid assinaturaId);
    }

    public class GatewayService : IGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly GatewaySettings _settings;
        private readonly ILogger<GatewayService> _logger;

        public GatewayService(
            HttpClient httpClient,
            IOptions<GatewaySettings> settings,
            ILogger<GatewayService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        /// <summary>
        /// Verifica licenca do usuario/cliente no gateway de pagamentos
        /// </summary>
        public async Task<GatewayLicenseResponse?> VerificarLicencaAsync(Guid? usuarioId = null, Guid? clienteId = null)
        {
            try
            {
                var request = new { usuario_id = usuarioId, cliente_id = clienteId };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/subscriptions/check", request);

                if (response.StatusCode == HttpStatusCode.PaymentRequired)
                {
                    // Licenca expirada ou nao encontrada
                    var error = await response.Content.ReadFromJsonAsync<GatewayErrorResponse>();
                    _logger.LogWarning("Licenca invalida: {Error}", error?.Error);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao verificar licenca: {StatusCode}", response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<GatewayLicenseResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexao com gateway de pagamentos");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout na chamada ao gateway de pagamentos");
                return null;
            }
        }

        /// <summary>
        /// Lista planos disponiveis no gateway
        /// </summary>
        public async Task<GatewayPlanoResponse[]> ListarPlanosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/planos");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao listar planos: {StatusCode}", response.StatusCode);
                    return Array.Empty<GatewayPlanoResponse>();
                }

                return await response.Content.ReadFromJsonAsync<GatewayPlanoResponse[]>()
                    ?? Array.Empty<GatewayPlanoResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar planos do gateway");
                return Array.Empty<GatewayPlanoResponse>();
            }
        }

        /// <summary>
        /// Cria assinatura com pagamento PIX no gateway
        /// </summary>
        public async Task<(GatewayAssinaturaPixResponse? Response, string? ErrorMessage)> CriarAssinaturaPixAsync(Guid planoId, Guid usuarioId, Guid? clienteId = null)
        {
            try
            {
                var request = new
                {
                    plano_id = planoId,
                    usuario_id = usuarioId,
                    cliente_id = clienteId ?? usuarioId // Se nao tem cliente, usa usuarioId
                };

                _logger.LogInformation("Criando assinatura PIX no gateway: PlanoId={PlanoId}, UsuarioId={UsuarioId}, ClienteId={ClienteId}, GatewayUrl={Url}",
                    planoId, usuarioId, clienteId, _httpClient.BaseAddress);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/assinaturas/pix", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao criar assinatura PIX: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return (null, $"Gateway retornou {response.StatusCode}: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GatewayAssinaturaPixResponse>();
                return (result, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexao com gateway");
                return (null, $"Erro de conexao com gateway: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar assinatura PIX no gateway");
                return (null, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria assinatura com pagamento Boleto no gateway
        /// </summary>
        public async Task<(GatewayAssinaturaBoletoResponse? Response, string? ErrorMessage)> CriarAssinaturaBoletoAsync(
            Guid planoId, Guid usuarioId, string nomePagador, string cpfCnpj, Guid? clienteId = null)
        {
            try
            {
                var request = new
                {
                    plano_id = planoId,
                    usuario_id = usuarioId,
                    cliente_id = clienteId ?? usuarioId,
                    nome_pagador = nomePagador,
                    cpf_cnpj = cpfCnpj
                };

                _logger.LogInformation("Criando assinatura Boleto no gateway: PlanoId={PlanoId}, UsuarioId={UsuarioId}",
                    planoId, usuarioId);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/assinaturas/boleto", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao criar assinatura Boleto: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return (null, $"Gateway retornou {response.StatusCode}: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GatewayAssinaturaBoletoResponse>();
                return (result, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexao com gateway");
                return (null, $"Erro de conexao com gateway: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar assinatura Boleto no gateway");
                return (null, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria assinatura com Pix Automatico (recorrencia) no gateway
        /// </summary>
        public async Task<(GatewayAssinaturaPixAutomaticoResponse? Response, string? ErrorMessage)> CriarAssinaturaPixAutomaticoAsync(
            Guid planoId, Guid usuarioId, string nomeDevedor, string cpfCnpj, string periodicidade = "ANUAL", Guid? clienteId = null)
        {
            try
            {
                var request = new
                {
                    plano_id = planoId,
                    usuario_id = usuarioId,
                    cliente_id = clienteId ?? usuarioId,
                    nome_devedor = nomeDevedor,
                    cpf_cnpj_devedor = cpfCnpj,
                    periodicidade = periodicidade
                };

                _logger.LogInformation("Criando assinatura Pix Automatico no gateway: PlanoId={PlanoId}, UsuarioId={UsuarioId}, Periodicidade={Periodicidade}",
                    planoId, usuarioId, periodicidade);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/assinaturas/pix-automatico", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao criar assinatura Pix Automatico: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return (null, $"Gateway retornou {response.StatusCode}: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GatewayAssinaturaPixAutomaticoResponse>();
                return (result, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexao com gateway");
                return (null, $"Erro de conexao com gateway: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar assinatura Pix Automatico no gateway");
                return (null, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria assinatura com pagamento Cartao de Credito no gateway
        /// </summary>
        public async Task<(GatewayAssinaturaCartaoResponse? Response, string? ErrorMessage)> CriarAssinaturaCartaoAsync(
            Guid planoId, Guid usuarioId, string paymentToken, int parcelas = 1, string bandeira = "", Guid? clienteId = null)
        {
            try
            {
                var request = new
                {
                    plano_id = planoId,
                    usuario_id = usuarioId,
                    cliente_id = clienteId ?? usuarioId,
                    payment_token = paymentToken,
                    parcelas = parcelas,
                    bandeira = bandeira
                };

                _logger.LogInformation("Criando assinatura Cartao no gateway: PlanoId={PlanoId}, UsuarioId={UsuarioId}",
                    planoId, usuarioId);

                var response = await _httpClient.PostAsJsonAsync("/api/v1/assinaturas/cartao", request);

                if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Pagamento com cartao recusado: {Error}", errorContent);
                    return (null, "Pagamento recusado. Verifique os dados do cartao e tente novamente.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erro ao criar assinatura Cartao: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return (null, $"Gateway retornou {response.StatusCode}: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GatewayAssinaturaCartaoResponse>();
                return (result, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexao com gateway");
                return (null, $"Erro de conexao com gateway: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar assinatura Cartao no gateway");
                return (null, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica status do pagamento de uma assinatura no gateway
        /// </summary>
        public async Task<GatewayVerificacaoPagamentoResponse?> VerificarPagamentoAsync(Guid assinaturaId)
        {
            try
            {
                _logger.LogInformation("Verificando pagamento no gateway: AssinaturaId={AssinaturaId}", assinaturaId);

                var response = await _httpClient.GetAsync($"/api/v1/assinaturas/{assinaturaId}/verificar-pagamento");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Erro ao verificar pagamento: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<GatewayVerificacaoPagamentoResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexao com gateway ao verificar pagamento");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar pagamento no gateway");
                return null;
            }
        }
    }

    #region DTOs para Gateway

    public class GatewayLicenseResponse
    {
        [JsonPropertyName("tem_licenca")]
        public bool TemLicenca { get; set; }

        [JsonPropertyName("licenca_ativa")]
        public bool LicencaAtiva { get; set; }

        [JsonPropertyName("status_mensagem")]
        public string StatusMensagem { get; set; } = string.Empty;

        [JsonPropertyName("dias_restantes")]
        public int DiasRestantes { get; set; }

        [JsonPropertyName("plano")]
        public GatewayPlanoResumoResponse? Plano { get; set; }
    }

    public class GatewayPlanoResumoResponse
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("limite_hectares")]
        public decimal LimiteHectares { get; set; }
    }

    public class GatewayPlanoResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [JsonPropertyName("valor_anual")]
        public decimal ValorAnual { get; set; }

        [JsonPropertyName("limite_hectares")]
        public decimal LimiteHectares { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }
    }

    public class GatewayErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("redirect_to")]
        public string RedirectTo { get; set; } = string.Empty;
    }

    public class GatewayAssinaturaPixResponse
    {
        [JsonPropertyName("assinatura")]
        public GatewayAssinaturaResponse? Assinatura { get; set; }

        [JsonPropertyName("pagamento_id")]
        public Guid PagamentoId { get; set; }

        [JsonPropertyName("pix_copia_cola")]
        public string? PixCopiaCola { get; set; }

        [JsonPropertyName("pix_qrcode_base64")]
        public string? PixQrCodeBase64 { get; set; }
    }

    public class GatewayAssinaturaBoletoResponse
    {
        [JsonPropertyName("assinatura")]
        public GatewayAssinaturaResponse? Assinatura { get; set; }

        [JsonPropertyName("pagamento_id")]
        public Guid PagamentoId { get; set; }

        [JsonPropertyName("boleto_codigo_barras")]
        public string? BoletoCodigoBarras { get; set; }

        [JsonPropertyName("boleto_url")]
        public string? BoletoUrl { get; set; }
    }

    public class GatewayAssinaturaPixAutomaticoResponse
    {
        [JsonPropertyName("assinatura")]
        public GatewayAssinaturaResponse? Assinatura { get; set; }

        [JsonPropertyName("pagamento_id")]
        public Guid PagamentoId { get; set; }

        [JsonPropertyName("id_rec")]
        public string? IdRec { get; set; }

        [JsonPropertyName("contrato")]
        public string? Contrato { get; set; }

        [JsonPropertyName("status_recorrencia")]
        public string? StatusRecorrencia { get; set; }

        [JsonPropertyName("pix_copia_cola")]
        public string? PixCopiaCola { get; set; }

        [JsonPropertyName("pix_qrcode_base64")]
        public string? PixQrCodeBase64 { get; set; }

        [JsonPropertyName("link_autorizacao")]
        public string? LinkAutorizacao { get; set; }
    }

    public class GatewayAssinaturaCartaoResponse
    {
        [JsonPropertyName("assinatura")]
        public GatewayAssinaturaResponse? Assinatura { get; set; }

        [JsonPropertyName("pagamento_id")]
        public Guid PagamentoId { get; set; }

        [JsonPropertyName("charge_id")]
        public string? ChargeId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("autorizacao")]
        public string? Autorizacao { get; set; }

        [JsonPropertyName("parcelas")]
        public int Parcelas { get; set; }
    }

    public class GatewayAssinaturaResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("cliente_id")]
        public Guid ClienteId { get; set; }

        [JsonPropertyName("usuario_id")]
        public Guid UsuarioId { get; set; }

        [JsonPropertyName("plano_id")]
        public Guid PlanoId { get; set; }

        [JsonPropertyName("plano_nome")]
        public string? PlanoNome { get; set; }

        [JsonPropertyName("data_inicio")]
        public DateTime DataInicio { get; set; }

        [JsonPropertyName("data_fim")]
        public DateTime DataFim { get; set; }

        [JsonPropertyName("ativa")]
        public bool Ativa { get; set; }

        [JsonPropertyName("status_pagamento")]
        public string? StatusPagamento { get; set; }

        [JsonPropertyName("dias_restantes")]
        public int DiasRestantes { get; set; }
    }

    public class GatewayVerificacaoPagamentoResponse
    {
        [JsonPropertyName("assinatura_id")]
        public Guid AssinaturaId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("pago")]
        public bool Pago { get; set; }

        [JsonPropertyName("assinatura_ativa")]
        public bool AssinaturaAtiva { get; set; }

        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        [JsonPropertyName("data_verificacao")]
        public DateTime DataVerificacao { get; set; }
    }

    #endregion
}
