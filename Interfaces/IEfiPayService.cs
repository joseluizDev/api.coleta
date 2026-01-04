using api.coleta.Models.DTOs.EfiPay;

namespace api.coleta.Interfaces
{
    public interface IEfiPayService
    {
        // Authentication
        Task<string> ObterAccessTokenAsync();

        // PIX Payments
        Task<EfiPayPixResponseDTO> CriarCobrancaPixAsync(
            decimal valor,
            string cpfCnpj,
            string nomeCliente,
            string? descricao = null
        );

        Task<EfiPayPixQrCodeDTO> ObterQrCodePixAsync(int locId);

        Task<EfiPayPixResponseDTO> ConsultarCobrancaPixAsync(string txid);

        // Webhook Processing
        Task ProcessarWebhookPixAsync(EfiPayPixWebhookDataDTO pixData);

        // Webhook Configuration (skip-mTLS)
        /// <summary>
        /// Cadastra webhook PIX na Efi Pay
        /// PUT /v2/webhook/:chave com header x-skip-mtls-checking: true
        /// </summary>
        Task<WebhookCadastroResultDTO> CadastrarWebhookPixAsync(string? webhookUrl = null);

        /// <summary>
        /// Consulta webhook PIX configurado na Efi Pay
        /// GET /v2/webhook/:chave
        /// </summary>
        Task<EfiPayWebhookResponseDTO?> ConsultarWebhookPixAsync();

        /// <summary>
        /// Remove webhook PIX da Efi Pay
        /// DELETE /v2/webhook/:chave
        /// </summary>
        Task<bool> RemoverWebhookPixAsync();

        // Health Check
        bool EstaConfigurado();
    }
}
