using System.Text.Json.Serialization;

namespace api.coleta.Models.DTOs.EfiPay
{
    // ===== PIX Request DTOs =====
    public class EfiPayPixCreateDTO
    {
        [JsonPropertyName("calendario")]
        public EfiPayCalendarioDTO Calendario { get; set; } = new();

        [JsonPropertyName("devedor")]
        public EfiPayDevedorDTO? Devedor { get; set; }

        [JsonPropertyName("valor")]
        public EfiPayValorDTO Valor { get; set; } = new();

        [JsonPropertyName("chave")]
        public string Chave { get; set; } = string.Empty;

        [JsonPropertyName("solicitacaoPagador")]
        public string? SolicitacaoPagador { get; set; }

        [JsonPropertyName("infoAdicionais")]
        public List<EfiPayInfoAdicionalDTO>? InfoAdicionais { get; set; }
    }

    public class EfiPayCalendarioDTO
    {
        [JsonPropertyName("expiracao")]
        public int Expiracao { get; set; } = 86400; // 24 hours in seconds

        [JsonPropertyName("criacao")]
        public string? Criacao { get; set; }
    }

    public class EfiPayDevedorDTO
    {
        [JsonPropertyName("cpf")]
        public string? Cpf { get; set; }

        [JsonPropertyName("cnpj")]
        public string? Cnpj { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;
    }

    public class EfiPayValorDTO
    {
        [JsonPropertyName("original")]
        public string Original { get; set; } = string.Empty;
    }

    public class EfiPayInfoAdicionalDTO
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("valor")]
        public string Valor { get; set; } = string.Empty;
    }

    // ===== PIX Response DTOs =====
    public class EfiPayPixResponseDTO
    {
        [JsonPropertyName("calendario")]
        public EfiPayCalendarioDTO? Calendario { get; set; }

        [JsonPropertyName("txid")]
        public string Txid { get; set; } = string.Empty;

        [JsonPropertyName("revisao")]
        public int Revisao { get; set; }

        [JsonPropertyName("loc")]
        public EfiPayLocDTO? Loc { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("devedor")]
        public EfiPayDevedorDTO? Devedor { get; set; }

        [JsonPropertyName("valor")]
        public EfiPayValorDTO? Valor { get; set; }

        [JsonPropertyName("chave")]
        public string Chave { get; set; } = string.Empty;

        [JsonPropertyName("pixCopiaECola")]
        public string? PixCopiaECola { get; set; }
    }

    public class EfiPayLocDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("tipoCob")]
        public string? TipoCob { get; set; }

        [JsonPropertyName("criacao")]
        public string? Criacao { get; set; }
    }

    public class EfiPayPixQrCodeDTO
    {
        [JsonPropertyName("imagemQrcode")]
        public string ImagemQrcode { get; set; } = string.Empty;

        [JsonPropertyName("qrcode")]
        public string Qrcode { get; set; } = string.Empty;
    }

    // ===== PIX Status =====
    public static class EfiPayPixStatus
    {
        public const string Ativa = "ATIVA";
        public const string Concluida = "CONCLUIDA";
        public const string RemovidaPeloUsuarioRecebedor = "REMOVIDA_PELO_USUARIO_RECEBEDOR";
        public const string RemovidaPeloPsp = "REMOVIDA_PELO_PSP";
    }
}
