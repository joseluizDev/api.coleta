using api.coleta.Data.Repository;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.EfiPay;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace api.coleta.Services
{
    public interface IEfiPayCartaoService
    {
        Task<PagamentoCartaoDTO> CriarCobrancaCartaoAsync(
            decimal valor,
            string paymentToken,
            int parcelas,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone,
            string? descricao = null);
        Task<ListaParcelasDTO> ObterParcelasAsync(string bandeira, decimal valorTotal);
        Task<bool> EstornarAsync(int chargeId, decimal? valor = null);
    }

    public class EfiPayCartaoService : IEfiPayCartaoService
    {
        private readonly EfiPaySettings _settings;
        private readonly ILogger<EfiPayCartaoService> _logger;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly IUnitOfWork _unitOfWork;

        // Token cache para API de Cobranças
        private static string? _cachedCobrancasToken;
        private static DateTime _cobrancasTokenExpiration = DateTime.MinValue;
        private static readonly object _cobrancasTokenLock = new();

        public EfiPayCartaoService(
            IOptions<EfiPaySettings> settings,
            ILogger<EfiPayCartaoService> logger,
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
        /// Obtém token OAuth2 para API de Cobranças
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
        /// Cria uma cobrança com cartão de crédito
        /// </summary>
        public async Task<PagamentoCartaoDTO> CriarCobrancaCartaoAsync(
            decimal valor,
            string paymentToken,
            int parcelas,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone,
            string? descricao = null)
        {
            if (string.IsNullOrEmpty(paymentToken))
            {
                throw new ArgumentException("Payment token é obrigatório para pagamento com cartão");
            }

            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Limpar CPF/CNPJ
            var cpfCnpjLimpo = cpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            var cartaoRequest = new EfiPayCartaoCreateDTO
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
                Payment = new EfiPayCartaoPaymentDTO
                {
                    CreditCard = new EfiPayCreditCardDTO
                    {
                        Customer = new EfiPayCustomerDTO
                        {
                            Name = nomeCliente,
                            Cpf = cpfCnpjLimpo.Length == 11 ? cpfCnpjLimpo : null,
                            Cnpj = cpfCnpjLimpo.Length == 14 ? cpfCnpjLimpo : null,
                            Email = email,
                            PhoneNumber = telefone?.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "")
                        },
                        Installments = parcelas,
                        PaymentToken = paymentToken
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(cartaoRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Criando cobrança cartão: {Request}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_settings.GetCobrancasBaseUrl()}/v1/charge/one-step", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("EfiPay create cartão failed: {StatusCode} - {Error}", response.StatusCode, responseContent);

                // Tentar extrair mensagem de erro específica
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<EfiPayCartaoResponseDTO>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (errorResponse?.Data?.Refusal != null)
                    {
                        return new PagamentoCartaoDTO
                        {
                            ChargeId = 0,
                            Status = "refused",
                            Valor = valor,
                            Parcelas = parcelas,
                            Aprovado = false,
                            MotivoRecusa = errorResponse.Data.Refusal.Reason,
                            PodeRetentar = errorResponse.Data.Refusal.Retry
                        };
                    }
                }
                catch { }

                throw new Exception($"Erro ao criar cobrança com cartão: {responseContent}");
            }

            var cartaoResponse = JsonSerializer.Deserialize<EfiPayCartaoResponseDTO>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (cartaoResponse?.Data == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao criar cobrança com cartão");
            }

            var data = cartaoResponse.Data;
            var aprovado = data.Status.ToLower() == "approved" || data.Status.ToLower() == "waiting";

            _logger.LogInformation("Cobrança cartão criada: ChargeId={ChargeId}, Status={Status}, Aprovado={Aprovado}",
                data.ChargeId, data.Status, aprovado);

            return new PagamentoCartaoDTO
            {
                ChargeId = data.ChargeId,
                Status = data.Status,
                Valor = valor,
                Parcelas = data.Installments,
                ValorParcela = data.InstallmentValue / 100m, // Converter de centavos
                Bandeira = ExtrairBandeira(data.Payment),
                Aprovado = aprovado,
                MotivoRecusa = data.Refusal?.Reason,
                PodeRetentar = data.Refusal?.Retry ?? false
            };
        }

        /// <summary>
        /// Obtém opções de parcelamento para uma bandeira
        /// </summary>
        public async Task<ListaParcelasDTO> ObterParcelasAsync(string bandeira, decimal valorTotal)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var valorCentavos = (int)(valorTotal * 100);

            var response = await client.GetAsync(
                $"{_settings.GetCobrancasBaseUrl()}/v1/installments?brand={bandeira.ToLower()}&total={valorCentavos}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay get installments failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao obter parcelas: {error}");
            }

            var installmentsResponse = await response.Content.ReadFromJsonAsync<EfiPayInstallmentsResponseDTO>();

            if (installmentsResponse?.Data == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao obter parcelas");
            }

            var parcelas = installmentsResponse.Data.Installments.Select(i => new ParcelaDTO
            {
                Numero = i.Installment,
                Valor = i.Value / 100m, // Converter de centavos
                ValorFormatado = $"R$ {(i.Value / 100m):N2}",
                TemJuros = i.HasInterest,
                PercentualJuros = i.InterestPercentage
            }).ToList();

            return new ListaParcelasDTO
            {
                Bandeira = installmentsResponse.Data.Name,
                Parcelas = parcelas
            };
        }

        /// <summary>
        /// Estorna total ou parcialmente uma cobrança de cartão
        /// </summary>
        public async Task<bool> EstornarAsync(int chargeId, decimal? valor = null)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            object requestBody = valor.HasValue
                ? new { amount = (int)(valor.Value * 100) }
                : new { };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"{_settings.GetCobrancasBaseUrl()}/v1/charge/{chargeId}/refund",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay refund failed: {StatusCode} - {Error}", response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Estorno realizado: ChargeId={ChargeId}, Valor={Valor}",
                chargeId, valor.HasValue ? valor.Value.ToString("C2") : "Total");

            return true;
        }

        #region Helpers

        /// <summary>
        /// Extrai bandeira do cartão da resposta
        /// </summary>
        private static string? ExtrairBandeira(string? payment)
        {
            if (string.IsNullOrEmpty(payment)) return null;

            // A resposta pode conter informações sobre a bandeira
            // Exemplo: "credit_card visa"
            var parts = payment.Split(' ');
            if (parts.Length > 1)
            {
                return parts[1];
            }

            return null;
        }

        #endregion
    }
}
