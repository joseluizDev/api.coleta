using System.Text;
using System.Text.Json;
using api.coleta.Interfaces;

namespace api.coleta.Services
{
    public class OneSignalService : IOneSignalService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _appId;
        private readonly string _apiUrl;

        public OneSignalService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            _apiKey = Environment.GetEnvironmentVariable("ONESIGNAL_API_KEY")
                ?? throw new InvalidOperationException("ONESIGNAL_API_KEY não configurado no .env");

            _appId = Environment.GetEnvironmentVariable("ONESIGNAL_APP_ID")
                ?? throw new InvalidOperationException("ONESIGNAL_APP_ID não configurado no .env");

            _apiUrl = Environment.GetEnvironmentVariable("ONESIGNAL_API_URL")
                ?? throw new InvalidOperationException("ONESIGNAL_API_URL não configurado no .env");
        }

        public async Task<bool> EnviarNotificacaoAsync(string fcmToken, string titulo, string mensagem)
        {
            try
            {
                if (string.IsNullOrEmpty(fcmToken))
                {
                    return false;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {_apiKey}");

                var payload = new
                {
                    app_id = _appId,
                    include_player_ids = new[] { fcmToken },
                    target_channel = "push",
                    contents = new { en = mensagem },
                    headings = new { en = titulo }
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_apiUrl, content);

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
