namespace api.coleta.Settings
{
    public class GatewaySettings
    {
        /// <summary>
        /// URL base do gateway de pagamentos Python
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:8001";

        /// <summary>
        /// API Key para autenticacao com o gateway
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Timeout em segundos para chamadas ao gateway
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(BaseUrl) && !string.IsNullOrEmpty(ApiKey);
        }
    }
}
