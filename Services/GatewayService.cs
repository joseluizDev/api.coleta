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

    #endregion
}
