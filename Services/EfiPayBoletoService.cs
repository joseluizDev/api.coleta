using api.coleta.Data.Repository;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.EfiPay;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Settings;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace api.coleta.Services
{
    public interface IEfiPayBoletoService
    {
        Task<PagamentoBoletoDTO> CriarBoletoAsync(
            decimal valor,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone,
            DateTime? vencimento = null,
            string? descricao = null);
        Task<EfiPayBoletoDataDTO?> ConsultarBoletoAsync(int chargeId);
        Task<bool> CancelarBoletoAsync(int chargeId);
    }

    public class EfiPayBoletoService : IEfiPayBoletoService
    {
        private readonly EfiPaySettings _settings;
        private readonly ILogger<EfiPayBoletoService> _logger;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly IUnitOfWork _unitOfWork;

        // Token cache para API de Cobranças (separado do PIX)
        private static string? _cachedCobrancasToken;
        private static DateTime _cobrancasTokenExpiration = DateTime.MinValue;
        private static readonly object _cobrancasTokenLock = new();

        public EfiPayBoletoService(
            IOptions<EfiPaySettings> settings,
            ILogger<EfiPayBoletoService> logger,
            HistoricoPagamentoRepository pagamentoRepo,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _pagamentoRepo = pagamentoRepo;
            _unitOfWork = unitOfWork;

            _settings = new EfiPaySettings
            {
                ClientId = "Client_Id_17711359c8b4a9ce370814111e98a3e1c4821443",
                ClientSecret = "Client_Secret_a1e623c3bd3f90262b377c9ab167def9b9d89234",
                ChavePix = "43f89047-906c-4876-b9d5-1c3149cbff95", // Chave aleatória de produção
                CertificadoPath = "/Volumes/MacOS/Trabalhos/Agro/api.coleta/producao-643354-AgroSyste.p12",
                WebhookUrl = "https://apis-api-coleta.w4dxlp.easypanel.host/api/webhook/efipay",
                UseSandbox = false, // Produção
            };
        }

        /// <summary>
        /// Obtém token OAuth2 para API de Cobranças (sem certificado mTLS)
        /// </summary>
        private async Task<string> ObterAccessTokenCobrancasAsync()
        {
            lock (_cobrancasTokenLock)
            {
                if (!string.IsNullOrEmpty(_cachedCobrancasToken) && DateTime.Now < _cobrancasTokenExpiration)
                {
                    return _cachedCobrancasToken;
                }
            }

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}")
            );

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(new { grant_type = "client_credentials" }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{_settings.GetCobrancasBaseUrl()}/v1/authorize", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay Cobranças auth failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Falha na autenticação EfiPay Cobranças: {error}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<EfiPayAuthTokenDTO>();

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new Exception("Resposta de token inválida do EfiPay Cobranças");
            }

            lock (_cobrancasTokenLock)
            {
                _cachedCobrancasToken = tokenResponse.AccessToken;
                _cobrancasTokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - 60);
            }

            _logger.LogInformation("EfiPay Cobranças token obtained successfully");
            return _cachedCobrancasToken;
        }

        /// <summary>
        /// Cria um boleto bancário
        /// </summary>
        public async Task<PagamentoBoletoDTO> CriarBoletoAsync(
            decimal valor,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone,
            DateTime? vencimento = null,
            string? descricao = null)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Vencimento padrão: 3 dias úteis
            var dataVencimento = vencimento ?? CalcularVencimento(3);

            // Limpar CPF/CNPJ
            var cpfCnpjLimpo = cpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            var boletoRequest = new EfiPayBoletoCreateDTO
            {
                Items = new List<EfiPayItemDTO>
                {
                    new EfiPayItemDTO
                    {
                        Name = descricao ?? "AgroSyste - Licença Anual",
                        Value = (int)(valor * 100), // Valor em centavos
                        Amount = 1
                    }
                },
                Payment = new EfiPayBoletoPaymentDTO
                {
                    BankingBillet = new EfiPayBankingBilletDTO
                    {
                        Customer = new EfiPayCustomerDTO
                        {
                            Name = nomeCliente,
                            Cpf = cpfCnpjLimpo.Length == 11 ? cpfCnpjLimpo : null,
                            Cnpj = cpfCnpjLimpo.Length == 14 ? cpfCnpjLimpo : null,
                            Email = email,
                            PhoneNumber = telefone?.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "")
                        },
                        ExpireAt = dataVencimento.ToString("yyyy-MM-dd"),
                        Configurations = new EfiPayBoletoConfigDTO
                        {
                            Fine = 200, // 2% de multa (em centavos = 200 = R$ 2,00)
                            Interest = new EfiPayInterestDTO
                            {
                                Value = 33, // 0,033% ao dia (1% ao mês)
                                Type = "monthly"
                            },
                            DaysToWriteOff = 30 // Baixa automática após 30 dias
                        },
                        Message = $"AgroSyste - Licença de uso do sistema. Valor: R$ {valor:N2}"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(boletoRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Criando boleto: {Request}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_settings.GetCobrancasBaseUrl()}/v1/charge/one-step", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay create boleto failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao criar boleto: {error}");
            }

            var boletoResponse = await response.Content.ReadFromJsonAsync<EfiPayBoletoResponseDTO>();

            if (boletoResponse?.Data == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao criar boleto");
            }

            _logger.LogInformation("Boleto criado: ChargeId={ChargeId}, Status={Status}",
                boletoResponse.Data.ChargeId, boletoResponse.Data.Status);

            return new PagamentoBoletoDTO
            {
                ChargeId = boletoResponse.Data.ChargeId,
                CodigoBarras = boletoResponse.Data.Barcode ?? string.Empty,
                LinhaDigitavel = ExtrairLinhaDigitavel(boletoResponse.Data.Barcode),
                PdfUrl = boletoResponse.Data.Pdf?.Charge ?? string.Empty,
                BoletoLink = boletoResponse.Data.BilletLink ?? boletoResponse.Data.Link ?? string.Empty,
                Valor = valor,
                DataVencimento = dataVencimento,
                PixQrCode = boletoResponse.Data.Pix?.Qrcode,
                PixQrCodeImagem = boletoResponse.Data.Pix?.QrcodeImage
            };
        }

        /// <summary>
        /// Consulta status de um boleto
        /// </summary>
        public async Task<EfiPayBoletoDataDTO?> ConsultarBoletoAsync(int chargeId)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_settings.GetCobrancasBaseUrl()}/v1/charge/{chargeId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay consult boleto failed: {StatusCode} - {Error}", response.StatusCode, error);
                return null;
            }

            var boletoResponse = await response.Content.ReadFromJsonAsync<EfiPayBoletoResponseDTO>();
            return boletoResponse?.Data;
        }

        /// <summary>
        /// Cancela um boleto pendente
        /// </summary>
        public async Task<bool> CancelarBoletoAsync(int chargeId)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsync(
                $"{_settings.GetCobrancasBaseUrl()}/v1/charge/{chargeId}/cancel",
                null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay cancel boleto failed: {StatusCode} - {Error}", response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Boleto cancelado: ChargeId={ChargeId}", chargeId);
            return true;
        }

        #region Helpers

        /// <summary>
        /// Calcula data de vencimento considerando dias úteis
        /// </summary>
        private static DateTime CalcularVencimento(int diasUteis)
        {
            var data = DateTime.Now;
            var diasAdicionados = 0;

            while (diasAdicionados < diasUteis)
            {
                data = data.AddDays(1);
                if (data.DayOfWeek != DayOfWeek.Saturday && data.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasAdicionados++;
                }
            }

            return data;
        }

        /// <summary>
        /// Extrai linha digitável do código de barras
        /// </summary>
        private static string ExtrairLinhaDigitavel(string? codigoBarras)
        {
            if (string.IsNullOrEmpty(codigoBarras)) return string.Empty;

            // A linha digitável já pode vir formatada ou precisar ser extraída
            // Formato: XXXXX.XXXXX XXXXX.XXXXXX XXXXX.XXXXXX X XXXXXXXXXXXXXX
            return codigoBarras.Replace(" ", "");
        }

        #endregion
    }
}
