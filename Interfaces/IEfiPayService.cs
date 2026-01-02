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

        // Health Check
        bool EstaConfigurado();
    }
}
