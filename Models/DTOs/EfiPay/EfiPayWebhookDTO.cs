using System.Text.Json.Serialization;

namespace api.coleta.Models.DTOs.EfiPay
{
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
}
