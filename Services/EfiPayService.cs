using api.coleta.Data.Repository;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.EfiPay;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Settings;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace api.coleta.Services
{
    public class EfiPayService : IEfiPayService
    {
        private readonly EfiPaySettings _settings;
        private readonly ILogger<EfiPayService> _logger;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly AssinaturaRepository _assinaturaRepo;
        private readonly IUnitOfWork _unitOfWork;

        // Token cache
        private static string? _cachedToken;
        private static DateTime _tokenExpiration = DateTime.MinValue;
        private static readonly object _tokenLock = new();

        public EfiPayService(
            IOptions<EfiPaySettings> settings,
            ILogger<EfiPayService> logger,
            HistoricoPagamentoRepository pagamentoRepo,
            AssinaturaRepository assinaturaRepo,
            IUnitOfWork unitOfWork)
        {
            _settings = settings.Value;
            _logger = logger;
            _pagamentoRepo = pagamentoRepo;
            _assinaturaRepo = assinaturaRepo;
            _unitOfWork = unitOfWork;

            _logger.LogInformation("EfiPayService initialized with CertificatePath: {Path}", _settings.CertificadoPath);
        }

        public bool EstaConfigurado()
        {
            return _settings.IsConfigured();
        }

        public async Task<string> ObterAccessTokenAsync()
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.Now < _tokenExpiration)
                {
                    return _cachedToken;
                }
            }

            using var client = CreateHttpClientWithCertificate();

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}")
            );

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Solicitar token com escopos PIX
            var tokenRequest = new
            {
                grant_type = "client_credentials",
                scope = "cob.write cob.read pix.write pix.read webhook.write webhook.read payloadlocation.write payloadlocation.read gn.pix.evp.write gn.pix.evp.read"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(tokenRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync($"{_settings.GetBaseUrl()}/oauth/token", content);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("prematurely") || ex.Message.Contains("ResponseEnded"))
            {
                _logger.LogError(ex, "EfiPay connection failed - likely certificate/credential mismatch. URL: {Url}", _settings.GetBaseUrl());
                throw new Exception("Falha na conexão com EfiPay. Verifique se o certificado corresponde ao ambiente (produção/homologação) e se as credenciais estão corretas.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay auth failed: {StatusCode} - {Error}", response.StatusCode, error);

                if (error.Contains("invalid_client") || error.Contains("Invalid or inactive credentials"))
                {
                    throw new Exception("Credenciais EfiPay inválidas ou inativas. Verifique o ClientId e ClientSecret no painel da EfiPay.");
                }

                throw new Exception($"Falha na autenticação EfiPay: {error}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<EfiPayAuthTokenDTO>();

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new Exception("Resposta de token inválida do EfiPay");
            }

            lock (_tokenLock)
            {
                _cachedToken = tokenResponse.AccessToken;
                _tokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - 60);
            }

            _logger.LogInformation("EfiPay token obtained successfully, expires in {ExpiresIn}s", tokenResponse.ExpiresIn);
            return _cachedToken;
        }

        public async Task<EfiPayPixResponseDTO> CriarCobrancaPixAsync(
            decimal valor,
            string cpfCnpj,
            string nomeCliente,
            string? descricao = null)
        {
            var token = await ObterAccessTokenAsync();
            using var client = CreateHttpClientWithCertificate();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var txid = GerarTxId();

            var pixData = new EfiPayPixCreateDTO
            {
                Calendario = new EfiPayCalendarioDTO { Expiracao = 86400 }, // 24h
                Devedor = new EfiPayDevedorDTO
                {
                    Cpf = cpfCnpj.Length == 11 ? cpfCnpj : null,
                    Cnpj = cpfCnpj.Length == 14 ? cpfCnpj : null,
                    Nome = nomeCliente
                },
                Valor = new EfiPayValorDTO
                {
                    Original = valor.ToString("F2", CultureInfo.InvariantCulture)
                },
                Chave = _settings.ChavePix,
                SolicitacaoPagador = descricao ?? "AgroSyste - Licença Anual"
            };

            var jsonContent = JsonSerializer.Serialize(pixData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_settings.GetBaseUrl()}/v2/cob/{txid}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay create PIX failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao criar cobrança PIX: {error}");
            }

            var pixResponse = await response.Content.ReadFromJsonAsync<EfiPayPixResponseDTO>();

            if (pixResponse == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao criar cobrança PIX");
            }

            _logger.LogInformation("PIX charge created: txid={Txid}, valor={Valor}", txid, valor);
            return pixResponse;
        }

        public async Task<EfiPayPixQrCodeDTO> ObterQrCodePixAsync(int locId)
        {
            var token = await ObterAccessTokenAsync();
            using var client = CreateHttpClientWithCertificate();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_settings.GetBaseUrl()}/v2/loc/{locId}/qrcode");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay get QR Code failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao obter QR Code PIX: {error}");
            }

            var qrCodeResponse = await response.Content.ReadFromJsonAsync<EfiPayPixQrCodeDTO>();

            if (qrCodeResponse == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao obter QR Code");
            }

            return qrCodeResponse;
        }

        public async Task<EfiPayPixResponseDTO> ConsultarCobrancaPixAsync(string txid)
        {
            var token = await ObterAccessTokenAsync();
            using var client = CreateHttpClientWithCertificate();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_settings.GetBaseUrl()}/v2/cob/{txid}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay consult PIX failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao consultar cobrança PIX: {error}");
            }

            var pixResponse = await response.Content.ReadFromJsonAsync<EfiPayPixResponseDTO>();

            if (pixResponse == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao consultar cobrança PIX");
            }

            return pixResponse;
        }

        public async Task ProcessarWebhookPixAsync(EfiPayPixWebhookDataDTO pixData)
        {
            _logger.LogInformation("Processing PIX webhook: txid={Txid}, valor={Valor}",
                pixData.Txid, pixData.Valor);

            // Find payment by txid
            var pagamento = await _pagamentoRepo.ObterPorPixTxIdAsync(pixData.Txid);

            if (pagamento == null)
            {
                _logger.LogWarning("PIX payment not found: txid={Txid}", pixData.Txid);
                return;
            }

            // Update payment status
            pagamento.Status = StatusPagamento.Aprovado;
            pagamento.EfiPayStatus = "paid";

            if (DateTime.TryParse(pixData.Horario, out var horario))
            {
                pagamento.DataPagamento = horario;
            }
            else
            {
                pagamento.DataPagamento = DateTime.Now;
            }

            _pagamentoRepo.Atualizar(pagamento);

            // Activate subscription
            var assinatura = await _assinaturaRepo.ObterPorIdAsync(pagamento.AssinaturaId);
            if (assinatura != null)
            {
                assinatura.Ativa = true;
                assinatura.StatusPagamento = "active";
                assinatura.DataUltimoPagamento = pagamento.DataPagamento;

                _assinaturaRepo.Atualizar(assinatura);
            }

            _unitOfWork.Commit();

            _logger.LogInformation("PIX payment confirmed and subscription activated: assinaturaId={AssinaturaId}",
                pagamento.AssinaturaId);
        }

        // ===== Private Helpers =====
        private HttpClient CreateHttpClientWithCertificate()
        {
            var handler = new HttpClientHandler();

            if (!File.Exists(_settings.CertificadoPath))
            {
                throw new Exception($"Certificado não encontrado: {_settings.CertificadoPath}");
            }

            try
            {
                // Carregar certificado sem senha (padrão EfiPay)
                var certificate = new X509Certificate2(_settings.CertificadoPath);
                handler.ClientCertificates.Add(certificate);
                _logger.LogDebug("Certificate loaded from {Path}, Subject: {Subject}", _settings.CertificadoPath, certificate.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar certificado");
                throw;
            }

            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private static string GerarTxId()
        {
            // Generate unique 26-35 char alphanumeric ID
            return Guid.NewGuid().ToString("N")[..32];
        }
    }
}
