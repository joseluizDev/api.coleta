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
    public interface IEfiPayAssinaturaService
    {
        Task<EfiPayPlanDataDTO> CriarPlanoEfiPayAsync(string nome, int intervaloMeses = 1, int? repeticoes = 12);
        Task<AssinaturaRecorrenteDTO> CriarAssinaturaRecorrenteCartaoAsync(
            int planoEfiPayId,
            decimal valorMensal,
            string paymentToken,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone);
        Task<AssinaturaRecorrenteDTO> CriarAssinaturaRecorrenteBoletoAsync(
            int planoEfiPayId,
            decimal valorMensal,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone);
        Task<AssinaturaRecorrenteDTO?> ConsultarAssinaturaAsync(int subscriptionId);
        Task<bool> CancelarAssinaturaAsync(int subscriptionId);
    }

    public class EfiPayAssinaturaService : IEfiPayAssinaturaService
    {
        private readonly EfiPaySettings _settings;
        private readonly ILogger<EfiPayAssinaturaService> _logger;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly IUnitOfWork _unitOfWork;

        // Token cache para API de Cobranças
        private static string? _cachedCobrancasToken;
        private static DateTime _cobrancasTokenExpiration = DateTime.MinValue;
        private static readonly object _cobrancasTokenLock = new();

        public EfiPayAssinaturaService(
            IOptions<EfiPaySettings> settings,
            ILogger<EfiPayAssinaturaService> logger,
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
                UseSandbox = false,
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
        /// Cria um plano de assinatura na EfiPay
        /// Deve ser chamado uma vez por plano do sistema (ex: Básico, Premium)
        /// </summary>
        public async Task<EfiPayPlanDataDTO> CriarPlanoEfiPayAsync(string nome, int intervaloMeses = 1, int? repeticoes = 12)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var planRequest = new EfiPayPlanCreateDTO
            {
                Name = nome,
                Interval = intervaloMeses,
                Repeats = repeticoes
            };

            var jsonContent = JsonSerializer.Serialize(planRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogDebug("Criando plano EfiPay: {Request}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_settings.GetCobrancasBaseUrl()}/v1/plan", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay create plan failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao criar plano na EfiPay: {error}");
            }

            var planResponse = await response.Content.ReadFromJsonAsync<EfiPayPlanResponseDTO>();

            if (planResponse?.Data == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao criar plano");
            }

            _logger.LogInformation("Plano EfiPay criado: PlanId={PlanId}, Nome={Nome}",
                planResponse.Data.PlanId, planResponse.Data.Name);

            return planResponse.Data;
        }

        /// <summary>
        /// Cria uma assinatura recorrente com cartão de crédito
        /// </summary>
        public async Task<AssinaturaRecorrenteDTO> CriarAssinaturaRecorrenteCartaoAsync(
            int planoEfiPayId,
            decimal valorMensal,
            string paymentToken,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone)
        {
            if (string.IsNullOrEmpty(paymentToken))
            {
                throw new ArgumentException("Payment token é obrigatório para assinatura com cartão");
            }

            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var cpfCnpjLimpo = cpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            var subscriptionRequest = new EfiPaySubscriptionCreateDTO
            {
                Items = new List<EfiPayItemDTO>
                {
                    new EfiPayItemDTO
                    {
                        Name = "AgroSyste - Mensalidade",
                        Value = (int)(valorMensal * 100), // Valor em centavos
                        Amount = 1
                    }
                },
                Payment = new EfiPaySubscriptionPaymentDTO
                {
                    CreditCard = new EfiPaySubscriptionCreditCardDTO
                    {
                        Customer = new EfiPayCustomerDTO
                        {
                            Name = nomeCliente,
                            Cpf = cpfCnpjLimpo.Length == 11 ? cpfCnpjLimpo : null,
                            Cnpj = cpfCnpjLimpo.Length == 14 ? cpfCnpjLimpo : null,
                            Email = email,
                            PhoneNumber = telefone?.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "")
                        },
                        PaymentToken = paymentToken
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(subscriptionRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Criando assinatura recorrente cartão: {Request}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(
                $"{_settings.GetCobrancasBaseUrl()}/v1/plan/{planoEfiPayId}/subscription/one-step",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay create subscription failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao criar assinatura recorrente: {error}");
            }

            var subscriptionResponse = await response.Content.ReadFromJsonAsync<EfiPaySubscriptionResponseDTO>();

            if (subscriptionResponse?.Data == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao criar assinatura");
            }

            var data = subscriptionResponse.Data;

            _logger.LogInformation("Assinatura recorrente criada: SubscriptionId={SubscriptionId}, Status={Status}",
                data.SubscriptionId, data.Status);

            return MapToAssinaturaRecorrenteDTO(data, valorMensal, "credit_card");
        }

        /// <summary>
        /// Cria uma assinatura recorrente com boleto
        /// </summary>
        public async Task<AssinaturaRecorrenteDTO> CriarAssinaturaRecorrenteBoletoAsync(
            int planoEfiPayId,
            decimal valorMensal,
            string cpfCnpj,
            string nomeCliente,
            string email,
            string? telefone)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var cpfCnpjLimpo = cpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            var dataVencimento = CalcularVencimento(10); // 10 dias para primeiro boleto

            var subscriptionRequest = new EfiPaySubscriptionCreateDTO
            {
                Items = new List<EfiPayItemDTO>
                {
                    new EfiPayItemDTO
                    {
                        Name = "AgroSyste - Mensalidade",
                        Value = (int)(valorMensal * 100), // Valor em centavos
                        Amount = 1
                    }
                },
                Payment = new EfiPaySubscriptionPaymentDTO
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
                            Fine = 200,
                            Interest = new EfiPayInterestDTO { Value = 33, Type = "monthly" }
                        },
                        Message = "AgroSyste - Mensalidade do sistema"
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(subscriptionRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Criando assinatura recorrente boleto: {Request}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(
                $"{_settings.GetCobrancasBaseUrl()}/v1/plan/{planoEfiPayId}/subscription/one-step",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay create subscription boleto failed: {StatusCode} - {Error}", response.StatusCode, error);
                throw new Exception($"Erro ao criar assinatura recorrente com boleto: {error}");
            }

            var subscriptionResponse = await response.Content.ReadFromJsonAsync<EfiPaySubscriptionResponseDTO>();

            if (subscriptionResponse?.Data == null)
            {
                throw new Exception("Resposta inválida do EfiPay ao criar assinatura");
            }

            var data = subscriptionResponse.Data;

            _logger.LogInformation("Assinatura recorrente boleto criada: SubscriptionId={SubscriptionId}, Status={Status}",
                data.SubscriptionId, data.Status);

            var result = MapToAssinaturaRecorrenteDTO(data, valorMensal, "banking_billet");

            // Adicionar dados do boleto se disponíveis
            if (data.Charge != null)
            {
                result.DadosBoleto = new PagamentoBoletoDTO
                {
                    ChargeId = data.Charge.Id,
                    CodigoBarras = data.Charge.Barcode ?? string.Empty,
                    BoletoLink = data.Charge.Link ?? string.Empty,
                    Valor = valorMensal,
                    DataVencimento = dataVencimento
                };
            }

            return result;
        }

        /// <summary>
        /// Consulta status de uma assinatura
        /// </summary>
        public async Task<AssinaturaRecorrenteDTO?> ConsultarAssinaturaAsync(int subscriptionId)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_settings.GetCobrancasBaseUrl()}/v1/subscription/{subscriptionId}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay consult subscription failed: {StatusCode} - {Error}", response.StatusCode, error);
                return null;
            }

            var subscriptionResponse = await response.Content.ReadFromJsonAsync<EfiPaySubscriptionResponseDTO>();

            if (subscriptionResponse?.Data == null)
            {
                return null;
            }

            // Valor mensal precisa ser calculado ou recuperado do contexto
            var valorMensal = subscriptionResponse.Data.Charge?.Total / 100m ?? 0;

            return MapToAssinaturaRecorrenteDTO(subscriptionResponse.Data, valorMensal, "unknown");
        }

        /// <summary>
        /// Cancela uma assinatura recorrente
        /// </summary>
        public async Task<bool> CancelarAssinaturaAsync(int subscriptionId)
        {
            var token = await ObterAccessTokenCobrancasAsync();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsync(
                $"{_settings.GetCobrancasBaseUrl()}/v1/subscription/{subscriptionId}/cancel",
                null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("EfiPay cancel subscription failed: {StatusCode} - {Error}", response.StatusCode, error);
                return false;
            }

            _logger.LogInformation("Assinatura cancelada: SubscriptionId={SubscriptionId}", subscriptionId);
            return true;
        }

        #region Helpers

        private static AssinaturaRecorrenteDTO MapToAssinaturaRecorrenteDTO(
            EfiPaySubscriptionDataDTO data,
            decimal valorMensal,
            string metodoPagamento)
        {
            DateTime? proximaCobranca = null;
            if (!string.IsNullOrEmpty(data.NextExecution) && DateTime.TryParse(data.NextExecution, out var nextExec))
            {
                proximaCobranca = nextExec;
            }

            DateTime? proximoVencimento = null;
            if (!string.IsNullOrEmpty(data.NextExpireAt) && DateTime.TryParse(data.NextExpireAt, out var nextExpire))
            {
                proximoVencimento = nextExpire;
            }

            return new AssinaturaRecorrenteDTO
            {
                SubscriptionId = data.SubscriptionId,
                Status = data.Status,
                PlanoNome = data.Plan?.Name ?? "Plano AgroSyste",
                ValorMensal = valorMensal,
                ParcelaAtual = data.Occurrences,
                TotalParcelas = data.Plan?.Repeats ?? 12,
                ProximaCobranca = proximaCobranca,
                ProximoVencimento = proximoVencimento,
                MetodoPagamento = metodoPagamento
            };
        }

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

        #endregion
    }
}
