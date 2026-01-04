using System.Text.Json.Serialization;

namespace api.coleta.Models.DTOs.EfiPay
{
    // ===== Plano DTOs =====

    public class EfiPayPlanCreateDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("interval")]
        public int Interval { get; set; } = 1; // Meses entre cobranças

        [JsonPropertyName("repeats")]
        public int? Repeats { get; set; } // Número de repetições (null = infinito)
    }

    public class EfiPayPlanResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public EfiPayPlanDataDTO? Data { get; set; }
    }

    public class EfiPayPlanDataDTO
    {
        [JsonPropertyName("plan_id")]
        public int PlanId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("interval")]
        public int Interval { get; set; }

        [JsonPropertyName("repeats")]
        public int? Repeats { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }
    }

    // ===== Assinatura (Subscription) DTOs =====

    public class EfiPaySubscriptionCreateDTO
    {
        [JsonPropertyName("items")]
        public List<EfiPayItemDTO> Items { get; set; } = new();

        [JsonPropertyName("payment")]
        public EfiPaySubscriptionPaymentDTO Payment { get; set; } = new();
    }

    public class EfiPaySubscriptionPaymentDTO
    {
        [JsonPropertyName("credit_card")]
        public EfiPaySubscriptionCreditCardDTO? CreditCard { get; set; }

        [JsonPropertyName("banking_billet")]
        public EfiPayBankingBilletDTO? BankingBillet { get; set; }
    }

    /// <summary>
    /// DTO de cartão de crédito específico para assinaturas recorrentes
    /// (sem campo installments que não é aceito pela EfiPay em assinaturas)
    /// </summary>
    public class EfiPaySubscriptionCreditCardDTO
    {
        [JsonPropertyName("customer")]
        public EfiPayCustomerDTO Customer { get; set; } = new();

        [JsonPropertyName("payment_token")]
        public string PaymentToken { get; set; } = string.Empty;

        [JsonPropertyName("billing_address")]
        public EfiPayAddressDTO? BillingAddress { get; set; }

        [JsonPropertyName("trial_days")]
        public int? TrialDays { get; set; }
    }

    public class EfiPaySubscriptionResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public EfiPaySubscriptionDataDTO? Data { get; set; }
    }

    public class EfiPaySubscriptionDataDTO
    {
        [JsonPropertyName("subscription_id")]
        public int SubscriptionId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("custom_id")]
        public string? CustomId { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("plan")]
        public EfiPayPlanDataDTO? Plan { get; set; }

        [JsonPropertyName("charge")]
        public EfiPaySubscriptionChargeDTO? Charge { get; set; }

        [JsonPropertyName("occurrences")]
        public int Occurrences { get; set; }

        [JsonPropertyName("next_execution")]
        public string? NextExecution { get; set; }

        [JsonPropertyName("next_expire_at")]
        public string? NextExpireAt { get; set; }
    }

    public class EfiPaySubscriptionChargeDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("parcel")]
        public int Parcel { get; set; }

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("payment")]
        public string? Payment { get; set; }
    }

    // ===== Lista de Assinaturas =====

    public class EfiPaySubscriptionListDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public List<EfiPaySubscriptionDataDTO>? Data { get; set; }
    }

    // ===== Cancelamento =====

    public class EfiPayCancelResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
    }

    // ===== DTOs para retorno ao Frontend =====

    public class AssinaturaRecorrenteDTO
    {
        public int SubscriptionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PlanoNome { get; set; } = string.Empty;
        public decimal ValorMensal { get; set; }
        public int ParcelaAtual { get; set; }
        public int TotalParcelas { get; set; }
        public DateTime? ProximaCobranca { get; set; }
        public DateTime? ProximoVencimento { get; set; }
        public string MetodoPagamento { get; set; } = string.Empty;
        public PagamentoCartaoDTO? DadosCartao { get; set; }
        public PagamentoBoletoDTO? DadosBoleto { get; set; }
    }

    public class AssinaturaCreateRecorrenteDTO
    {
        public Guid PlanoId { get; set; }
        public Guid ClienteId { get; set; }
        public string MetodoPagamento { get; set; } = "credit_card"; // credit_card ou banking_billet
        public string? PaymentToken { get; set; } // Obrigatório se cartão
        public string? CpfCnpj { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }

    public class AssinaturaComRecorrenciaDTO
    {
        public Licenciamento.AssinaturaDTO Assinatura { get; set; } = null!;
        public AssinaturaRecorrenteDTO? Recorrencia { get; set; }
    }
}
