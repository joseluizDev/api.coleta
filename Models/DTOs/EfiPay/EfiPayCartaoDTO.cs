using System.Text.Json.Serialization;

namespace api.coleta.Models.DTOs.EfiPay
{
    // ===== Request DTOs =====

    public class EfiPayCartaoCreateDTO
    {
        [JsonPropertyName("items")]
        public List<EfiPayItemDTO> Items { get; set; } = new();

        [JsonPropertyName("payment")]
        public EfiPayCartaoPaymentDTO Payment { get; set; } = new();
    }

    public class EfiPayCartaoPaymentDTO
    {
        [JsonPropertyName("credit_card")]
        public EfiPayCreditCardDTO CreditCard { get; set; } = new();
    }

    public class EfiPayCreditCardDTO
    {
        [JsonPropertyName("customer")]
        public EfiPayCustomerDTO Customer { get; set; } = new();

        [JsonPropertyName("installments")]
        public int Installments { get; set; } = 1;

        [JsonPropertyName("payment_token")]
        public string PaymentToken { get; set; } = string.Empty;

        [JsonPropertyName("billing_address")]
        public EfiPayAddressDTO? BillingAddress { get; set; }

        [JsonPropertyName("trial_days")]
        public int? TrialDays { get; set; }
    }

    // ===== Response DTOs =====

    public class EfiPayCartaoResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public EfiPayCartaoDataDTO? Data { get; set; }
    }

    public class EfiPayCartaoDataDTO
    {
        [JsonPropertyName("charge_id")]
        public int ChargeId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("installments")]
        public int Installments { get; set; }

        [JsonPropertyName("installment_value")]
        public int InstallmentValue { get; set; }

        [JsonPropertyName("payment")]
        public string? Payment { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("refusal")]
        public EfiPayRefusalDTO? Refusal { get; set; }
    }

    public class EfiPayRefusalDTO
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("retry")]
        public bool Retry { get; set; }
    }

    // ===== Parcelas DTOs =====

    public class EfiPayInstallmentsResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public EfiPayInstallmentsDataDTO? Data { get; set; }
    }

    public class EfiPayInstallmentsDataDTO
    {
        [JsonPropertyName("rate")]
        public int Rate { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("installments")]
        public List<EfiPayInstallmentDTO> Installments { get; set; } = new();
    }

    public class EfiPayInstallmentDTO
    {
        [JsonPropertyName("installment")]
        public int Installment { get; set; }

        [JsonPropertyName("has_interest")]
        public bool HasInterest { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("interest_percentage")]
        public decimal InterestPercentage { get; set; }
    }

    // ===== Estorno DTO =====

    public class EfiPayRefundRequestDTO
    {
        [JsonPropertyName("amount")]
        public int? Amount { get; set; } // Em centavos, null = estorno total
    }

    public class EfiPayRefundResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    // ===== DTO para retorno ao Frontend =====

    public class PagamentoCartaoDTO
    {
        public int ChargeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int Parcelas { get; set; }
        public decimal ValorParcela { get; set; }
        public string? Bandeira { get; set; }
        public string? Ultimos4Digitos { get; set; }
        public bool Aprovado { get; set; }
        public string? MotivoRecusa { get; set; }
        public bool PodeRetentar { get; set; }
    }

    public class ParcelaDTO
    {
        public int Numero { get; set; }
        public decimal Valor { get; set; }
        public string ValorFormatado { get; set; } = string.Empty;
        public bool TemJuros { get; set; }
        public decimal PercentualJuros { get; set; }
    }

    public class ListaParcelasDTO
    {
        public string Bandeira { get; set; } = string.Empty;
        public List<ParcelaDTO> Parcelas { get; set; } = new();
    }
}
