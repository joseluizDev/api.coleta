using System.Text.Json.Serialization;

namespace api.coleta.Models.DTOs.EfiPay
{
    // ===== WEBHOOK NOTIFICATION DTOs =====

    public class EfiPayWebhookNotificationDTO
    {
        [JsonPropertyName("pix")]
        public List<EfiPayPixWebhookDataDTO>? Pix { get; set; }
    }

    public class EfiPayPixWebhookDataDTO
    {
        [JsonPropertyName("endToEndId")]
        public string EndToEndId { get; set; } = string.Empty;

        [JsonPropertyName("txid")]
        public string Txid { get; set; } = string.Empty;

        [JsonPropertyName("valor")]
        public string Valor { get; set; } = string.Empty;

        [JsonPropertyName("horario")]
        public string Horario { get; set; } = string.Empty;

        [JsonPropertyName("infoPagador")]
        public string? InfoPagador { get; set; }

        [JsonPropertyName("pagador")]
        public EfiPayDevedorDTO? Pagador { get; set; }
    }

    // ===== WEBHOOK CONFIGURATION DTOs =====

    /// <summary>
    /// DTO para cadastrar/atualizar webhook PIX
    /// PUT /v2/webhook/:chave
    /// </summary>
    public class EfiPayWebhookConfigDTO
    {
        [JsonPropertyName("webhookUrl")]
        public string WebhookUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta ao consultar webhook configurado
    /// GET /v2/webhook/:chave
    /// </summary>
    public class EfiPayWebhookResponseDTO
    {
        [JsonPropertyName("webhookUrl")]
        public string? WebhookUrl { get; set; }

        [JsonPropertyName("chave")]
        public string? Chave { get; set; }

        [JsonPropertyName("criacao")]
        public string? Criacao { get; set; }
    }

    /// <summary>
    /// Resultado do cadastro de webhook
    /// </summary>
    public class WebhookCadastroResultDTO
    {
        public bool Sucesso { get; set; }
        public string? Mensagem { get; set; }
        public string? WebhookUrl { get; set; }
        public string? ChavePix { get; set; }
        public string? DataCadastro { get; set; }
        public string? Erro { get; set; }
    }
}
