using System.Text.Json.Serialization;

namespace api.coleta.Models.DTOs.EfiPay
{
    // ===== Request DTOs =====

    public class EfiPayBoletoCreateDTO
    {
        [JsonPropertyName("items")]
        public List<EfiPayItemDTO> Items { get; set; } = new();

        [JsonPropertyName("payment")]
        public EfiPayBoletoPaymentDTO Payment { get; set; } = new();
    }

    public class EfiPayItemDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public int Value { get; set; } // Em centavos

        [JsonPropertyName("amount")]
        public int Amount { get; set; } = 1;
    }

    public class EfiPayBoletoPaymentDTO
    {
        [JsonPropertyName("banking_billet")]
        public EfiPayBankingBilletDTO BankingBillet { get; set; } = new();
    }

    public class EfiPayBankingBilletDTO
    {
        [JsonPropertyName("customer")]
        public EfiPayCustomerDTO Customer { get; set; } = new();

        [JsonPropertyName("expire_at")]
        public string ExpireAt { get; set; } = string.Empty; // YYYY-MM-DD

        [JsonPropertyName("configurations")]
        public EfiPayBoletoConfigDTO? Configurations { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class EfiPayCustomerDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cpf")]
        public string? Cpf { get; set; }

        [JsonPropertyName("cnpj")]
        public string? Cnpj { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("birth")]
        public string? Birth { get; set; } // YYYY-MM-DD

        [JsonPropertyName("address")]
        public EfiPayAddressDTO? Address { get; set; }
    }

    public class EfiPayAddressDTO
    {
        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public string Number { get; set; } = string.Empty;

        [JsonPropertyName("neighborhood")]
        public string Neighborhood { get; set; } = string.Empty;

        [JsonPropertyName("zipcode")]
        public string Zipcode { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("complement")]
        public string? Complement { get; set; }
    }

    public class EfiPayBoletoConfigDTO
    {
        [JsonPropertyName("fine")]
        public int? Fine { get; set; } // Em centavos

        [JsonPropertyName("interest")]
        public EfiPayInterestDTO? Interest { get; set; }

        [JsonPropertyName("days_to_write_off")]
        public int? DaysToWriteOff { get; set; }
    }

    public class EfiPayInterestDTO
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "monthly"; // monthly ou daily
    }

    // ===== Response DTOs =====

    public class EfiPayBoletoResponseDTO
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public EfiPayBoletoDataDTO? Data { get; set; }
    }

    public class EfiPayBoletoDataDTO
    {
        [JsonPropertyName("charge_id")]
        public int ChargeId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("pix")]
        public EfiPayBoletoPix? Pix { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("billet_link")]
        public string? BilletLink { get; set; }

        [JsonPropertyName("pdf")]
        public EfiPayBoletoPdfDTO? Pdf { get; set; }

        [JsonPropertyName("expire_at")]
        public string? ExpireAt { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("payment")]
        public string? Payment { get; set; }

        [JsonPropertyName("custom_id")]
        public string? CustomId { get; set; }
    }

    public class EfiPayBoletoPix
    {
        [JsonPropertyName("qrcode")]
        public string? Qrcode { get; set; }

        [JsonPropertyName("qrcode_image")]
        public string? QrcodeImage { get; set; }
    }

    public class EfiPayBoletoPdfDTO
    {
        [JsonPropertyName("charge")]
        public string? Charge { get; set; } // Base64 encoded PDF
    }

    // ===== DTO para retorno ao Frontend =====

    public class PagamentoBoletoDTO
    {
        public int ChargeId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string LinhaDigitavel { get; set; } = string.Empty;
        public string PdfUrl { get; set; } = string.Empty;
        public string BoletoLink { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public string? PixQrCode { get; set; }
        public string? PixQrCodeImagem { get; set; }
    }
}
