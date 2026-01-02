namespace api.coleta.Settings
{
    public class EfiPaySettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string ChavePix { get; set; } = string.Empty;
        public string CertificadoPath { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://pix.api.efipay.com.br";
        public bool UseSandbox { get; set; } = false;

        // URLs da API PIX (requer certificado mTLS)
        public string GetBaseUrl()
        {
            return UseSandbox
                ? "https://pix-h.api.efipay.com.br"
                : BaseUrl;
        }

        // URLs da API de Cobranças (Boleto, Cartão, Assinaturas - sem certificado)
        public string GetCobrancasBaseUrl()
        {
            return UseSandbox
                ? "https://cobrancas-h.api.efipay.com.br"
                : "https://cobrancas.api.efipay.com.br";
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(ClientId)
                && !string.IsNullOrEmpty(ClientSecret)
                && !string.IsNullOrEmpty(ChavePix);
        }
    }
}
