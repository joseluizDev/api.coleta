using System.Text;
using System.Text.Json;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs;

namespace api.coleta.Services
{
    public class ZeptomailService : IZeptomailService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private const string ZEPTOMAIL_URL = "https://api.zeptomail.com/v1.1/email";
        private const string FROM_EMAIL = "noreply@agrosyste.com.br";
        private const string WHATSAPP_LINK = "https://wa.me/5599984989759";
        private readonly string[] ADMIN_EMAILS = new[]
        {
            "hpratesandrade@gmail.com",
            "gabybezerra10@gmail.com",
            "esteticistasabrina2@gmail.com",
            "luizzzz1996@gmail.com"
        };

        public ZeptomailService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["Zeptomail:ApiKey"]
                ?? throw new InvalidOperationException("Zeptomail API Key não configurada.");
        }

        public async Task<bool> EnviarEmailConfirmacaoAsync(string nomeUsuario, string emailUsuario)
        {
            try
            {
                var htmlBody = BuildConfirmacaoTemplate(nomeUsuario);
                var subject = "Obrigado pelo seu interesse no AgroSyste!";

                return await EnviarEmailAsync(emailUsuario, nomeUsuario, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email de confirmação: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailNotificacaoAdminsAsync(ContatoRequestDTO contato)
        {
            try
            {
                var htmlBody = BuildNotificacaoTemplate(contato);
                var subject = $"Novo Contato: {contato.NomeCompleto}";

                var tasks = ADMIN_EMAILS.Select(adminEmail =>
                    EnviarEmailAsync(adminEmail, "Administrador", subject, htmlBody)
                );

                var results = await Task.WhenAll(tasks);
                return results.All(r => r);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email para administradores: {ex.Message}");
                return false;
            }
        }

        private string BuildConfirmacaoTemplate(string nomeUsuario)
        {
            return $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""UTF-8"">
  <title>AgroSyste | Entraremos em Contato</title>
</head>
<body style=""margin:0; padding:0; background-color:#f4f6f8; font-family: Arial, Helvetica, sans-serif;"">

  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f8; padding:20px;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff; border-radius:6px; overflow:hidden;"">

          <!-- Título -->
          <tr>
            <td style=""background-color:#1f7a4d; padding:14px 14px 6px; text-align:center;"">
              <h1 style=""margin:0; color:#ffffff; font-size:22px; font-weight:normal;"">
                Entraremos em contato
              </h1>
            </td>
          </tr>

          <!-- Logo abaixo do título -->
          <tr>
            <td style=""background-color:#1f7a4d; padding:6px 0 14px; text-align:center;"">
            </td>
          </tr>

          <!-- Conteúdo -->
          <tr>
            <td style=""padding:28px; color:#333333; font-size:15px; line-height:1.6;"">
              <img src=""https://www.agrosyste.com/assets/logo.png""
                   alt=""AgroSyste""
                   width=""80""
                   style=""display:block; margin:0 auto; opacity:0.95;"">
              <p>Olá, <strong>{nomeUsuario}</strong>!</p>

              <p>
                Identificamos que você demonstrou interesse em conhecer melhor as soluções da
                <strong>AgroSyste</strong>.
                Nossa equipe <strong>entrará em contato em breve</strong> para entender suas necessidades
                e apresentar soluções que podem otimizar sua operação agrícola.
              </p>

              <p>
                Caso queira <strong>acelerar o atendimento</strong>, você pode entrar em contato diretamente
                conosco pelo <strong>WhatsApp</strong>.
              </p>

              <!-- Botão WhatsApp centralizado -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0 8px 0;"">
                <tr>
                  <td align=""center"">
                    <table cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""background-color:#25D366; border-radius:4px; text-align:center;"">
                          <a href=""{WHATSAPP_LINK}""
                             style=""display:inline-block; padding:14px 32px; color:#ffffff; text-decoration:none; font-size:15px; font-weight:bold;"">
                            Falar no WhatsApp
                          </a>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>

              <p style=""text-align:center; font-size:14px; color:#555555; margin-top:6px;"">
                WhatsApp: <strong>+55 (99) 98498-9759</strong>
              </p>

              <p>
                Se você já estiver em contato com alguém da nossa equipe, por favor, desconsidere esta mensagem.
              </p>

              <p>
                Atenciosamente,<br>
                <strong>Equipe AgroSyste</strong>
              </p>
            </td>
          </tr>

          <!-- Rodapé -->
          <tr>
            <td style=""background-color:#f0f3f5; padding:16px; text-align:center; font-size:12px; color:#777777;"">
              © 2025 AgroSyste · Todos os direitos reservados.<br>
              Esta é uma mensagem automática, por favor não responda diretamente.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>

</body>
</html>";
        }

        private string BuildNotificacaoTemplate(ContatoRequestDTO contato)
        {
            var telefoneWhatsApp = contato.NumeroTelefone
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            return $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""UTF-8"">
  <title>Novo Contato - AgroSyste</title>
</head>
<body style=""margin:0; padding:0; background-color:#f4f6f8; font-family: Arial, Helvetica, sans-serif;"">

  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f8; padding:20px;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff; border-radius:6px; overflow:hidden;"">

          <!-- Header -->
          <tr>
            <td style=""background-color:#1f7a4d; padding:20px; text-align:center;"">
              <h1 style=""margin:0; color:#ffffff; font-size:22px; font-weight:normal;"">
                Novo Contato Recebido
              </h1>
            </td>
          </tr>

          <!-- Conteúdo -->
          <tr>
            <td style=""padding:28px; color:#333333; font-size:15px; line-height:1.6;"">
              <p style=""margin-top:0;"">
                Um novo contato foi registrado através do formulário do site:
              </p>

              <table width=""100%"" cellpadding=""12"" cellspacing=""0"" style=""border:1px solid #e0e0e0; border-radius:4px; margin:20px 0;"">
                <tr style=""background-color:#f8f9fa;"">
                  <td style=""font-weight:bold; width:150px; border-bottom:1px solid #e0e0e0;"">Nome Completo:</td>
                  <td style=""border-bottom:1px solid #e0e0e0;"">{contato.NomeCompleto}</td>
                </tr>
                <tr>
                  <td style=""font-weight:bold; background-color:#f8f9fa; border-bottom:1px solid #e0e0e0;"">Cidade:</td>
                  <td style=""border-bottom:1px solid #e0e0e0;"">{contato.Cidade}</td>
                </tr>
                <tr style=""background-color:#f8f9fa;"">
                  <td style=""font-weight:bold; border-bottom:1px solid #e0e0e0;"">Email:</td>
                  <td style=""border-bottom:1px solid #e0e0e0;"">
                    <a href=""mailto:{contato.Email}"" style=""color:#1f7a4d;"">{contato.Email}</a>
                  </td>
                </tr>
                <tr>
                  <td style=""font-weight:bold; background-color:#f8f9fa;"">Telefone:</td>
                  <td>
                    <a href=""tel:{contato.NumeroTelefone}"" style=""color:#1f7a4d;"">{contato.NumeroTelefone}</a>
                  </td>
                </tr>
              </table>

              <p style=""background-color:#fff3cd; border-left:4px solid #ffc107; padding:15px; border-radius:4px; margin:20px 0;"">
                <strong>Ação necessária:</strong> Entre em contato com o cliente o mais breve possível.
              </p>

              <!-- Botão WhatsApp -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0;"">
                <tr>
                  <td align=""center"">
                    <table cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""background-color:#25D366; border-radius:4px; text-align:center;"">
                          <a href=""https://wa.me/55{telefoneWhatsApp}""
                             style=""display:inline-block; padding:14px 32px; color:#ffffff; text-decoration:none; font-size:15px; font-weight:bold;"">
                            Contatar via WhatsApp
                          </a>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Rodapé -->
          <tr>
            <td style=""background-color:#f0f3f5; padding:16px; text-align:center; font-size:12px; color:#777777;"">
              © 2025 AgroSyste - Sistema Interno<br>
              Esta é uma mensagem automática do sistema.
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>

</body>
</html>";
        }

        private async Task<bool> EnviarEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Zoho-enczapikey {_apiKey}");

                var payload = new
                {
                    from = new { address = FROM_EMAIL },
                    to = new[]
                    {
                        new
                        {
                            email_address = new
                            {
                                address = toEmail,
                                name = toName
                            }
                        }
                    },
                    subject = subject,
                    htmlbody = htmlBody
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(ZEPTOMAIL_URL, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Email enviado com sucesso para {toEmail}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erro ao enviar email para {toEmail}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao enviar email para {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
