using System.Text;
using System.Text.Json;
using api.coleta.Interfaces;

namespace api.coleta.Services
{
    public class OneSignalService : IOneSignalService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OneSignalService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<bool> EnviarNotificacaoAsync(string fcmToken, string titulo, string mensagem)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                {
                    return false;
                }

                var apiKey = _configuration["OneSignal:ApiKey"];
                var appId = _configuration["OneSignal:AppId"];

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(appId))
                {
                    throw new InvalidOperationException("OneSignal API Key ou App ID não configurados.");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {apiKey}");

                var payload = new
                {
                    app_id = appId,
                    include_player_ids = new[] { fcmToken },
                    target_channel = "push",
                    contents = new { en = mensagem },
                    headings = new { en = titulo }
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.onesignal.com/notifications?c=push", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erro ao enviar notificação: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao enviar notificação: {ex.Message}");
                return false;
            }
        }
    }
}
