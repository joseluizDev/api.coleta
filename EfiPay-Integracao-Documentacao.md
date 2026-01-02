# Documentacao de Integracao Efi Pay (EfiBank)

Documentacao tecnica para integracao com a API da Efi Pay (antigo Gerencianet/EfiBank) em .NET/C#.

---

## Indice

1. [Visao Geral](#visao-geral)
2. [Pre-requisitos](#pre-requisitos)
3. [Configuracao](#configuracao)
4. [Autenticacao OAuth2](#autenticacao-oauth2)
5. [Implementacao Base](#implementacao-base)
6. [Cobrancas PIX](#cobrancas-pix)
7. [Cobranca Recorrente (Assinaturas)](#cobranca-recorrente-assinaturas)
8. [Webhooks](#webhooks)
9. [Models e DTOs](#models-e-dtos)
10. [Referencias](#referencias)

---

## Visao Geral

A Efi Pay oferece APIs para:
- **PIX**: Cobrancas instantaneas, QR Code, transferencias
- **Boleto**: Emissao de boletos bancarios
- **Cartao de Credito**: Cobrancas avulsas e recorrentes
- **Assinaturas**: Cobrancas recorrentes automaticas (ideal para licenciamento de software)

### URLs Base

| Ambiente   | PIX API                           | Cobrancas API                          |
|------------|-----------------------------------|----------------------------------------|
| Producao   | `https://pix.api.efipay.com.br`   | `https://cobrancas.api.efipay.com.br`  |
| Sandbox    | `https://pix-h.api.efipay.com.br` | `https://cobrancas-h.api.efipay.com.br`|

---

## Pre-requisitos

1. **Conta Efi**: Criar conta em [sejaefi.com.br](https://sejaefi.com.br)
2. **Aplicacao API**: Criar aplicacao no painel da Efi
3. **Credenciais**: Obter `Client_Id` e `Client_Secret`
4. **Certificado .p12**: Necessario para API PIX (mTLS)
5. **Habilitar Escopos**: Ativar APIs necessarias (PIX, Cobrancas, etc.)

---

## Configuracao

### appsettings.json

```json
{
  "EfiPay": {
    "ClientId": "SEU_CLIENT_ID",
    "ClientSecret": "SEU_CLIENT_SECRET",
    "ChavePix": "sua-chave-pix@email.com",
    "CertificadoPath": "C:/certificados/certificado-efi.p12",
    "WebhookUrl": "https://seudominio.com/api/webhook/pix",
    "BaseURL": "https://pix.api.efipay.com.br"
  }
}
```

### Classe de Settings

```csharp
public class EfiPaySettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ChavePix { get; set; } = string.Empty;
    public string CertificadoPath { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string? BaseURL { get; set; }
}
```

### Registro no DI Container

```csharp
// Program.cs ou Startup.cs
services.Configure<EfiPaySettings>(configuration.GetSection("EfiPay"));
services.AddHttpClient<IEfiPayService, EfiPayService>();
```

---

## Autenticacao OAuth2

A API usa OAuth2 com Basic Auth. O token tem validade de **600 segundos**.

### Fluxo de Autenticacao

```
┌─────────────┐      POST /oauth/token      ┌─────────────┐
│   Cliente   │ ─────────────────────────── │   Efi API   │
│             │  Authorization: Basic xxx   │             │
│             │ ◄─────────────────────────  │             │
│             │      access_token           │             │
└─────────────┘                             └─────────────┘
```

### Implementacao

```csharp
private async Task<string> ObterTokenAcessoAsync()
{
    // Verifica cache do token
    if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiration)
    {
        return _accessToken;
    }

    using var client = CriarHttpClientComCertificado();

    // Monta Basic Auth
    var authString = $"{_settings.ClientId}:{_settings.ClientSecret}";
    var authBytes = Encoding.ASCII.GetBytes(authString);
    var authBase64 = Convert.ToBase64String(authBytes);

    var requestBody = new { grant_type = "client_credentials" };
    var jsonBody = JsonSerializer.Serialize(requestBody);

    var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/oauth/token")
    {
        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
    };
    request.Headers.Add("Authorization", $"Basic {authBase64}");

    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonSerializer.Deserialize<EfiPayTokenResponse>(content);

    _accessToken = tokenResponse.AccessToken;
    _tokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - 60); // Margem de 60s

    return _accessToken;
}
```

### Model do Token

```csharp
public class EfiPayTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
```

---

## Implementacao Base

### HttpClient com Certificado (mTLS)

Para API PIX, e necessario usar certificado digital:

```csharp
private HttpClient CriarHttpClientComCertificado()
{
    var handler = new HttpClientHandler();

    if (!File.Exists(_settings.CertificadoPath))
    {
        throw new Exception($"Certificado nao encontrado: {_settings.CertificadoPath}");
    }

    var certificate = new X509Certificate2(_settings.CertificadoPath);
    handler.ClientCertificates.Add(certificate);

    return new HttpClient(handler)
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
}
```

### Interface do Servico

```csharp
public interface IEfiPayService
{
    // PIX - Cobrancas
    Task<RespostaPixDto> GerarCobrancaPixAsync(SolicitacaoRecargaDto solicitacao);
    Task<RespostaPixDto> ConsultarCobrancaPixAsync(string txid);
    Task<RespostaPixDto> CancelarCobrancaPixAsync(string txid);

    // PIX - Transferencias
    Task<RespostaTransferenciaDto> ConsultarStatusTransferenciaAsync(string idTransferencia);

    // Webhook
    Task<bool> ProcessarWebhookRecargaPixAsync(string jsonPayload);
}
```

---

## Cobrancas PIX

### Criar Cobranca PIX (QR Code)

```csharp
public async Task<RespostaPixDto> GerarCobrancaPixAsync(SolicitacaoRecargaDto solicitacao)
{
    var txid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 32);
    var token = await ObterTokenAcessoAsync();

    var requestBody = new
    {
        calendario = new { expiracao = 3600 }, // 1 hora
        devedor = new
        {
            cnpj = solicitacao.CnpjLoja,
            nome = solicitacao.NomeLoja
        },
        valor = new
        {
            original = solicitacao.Valor.ToString("F2", CultureInfo.InvariantCulture)
        },
        solicitacaoPagador = "Descricao da cobranca",
        chave = _settings.ChavePix
    };

    using var client = CriarHttpClientComCertificado();

    var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/v2/cob/{txid}")
    {
        Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
    };
    request.Headers.Add("Authorization", $"Bearer {token}");

    var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    var cobResponse = JsonSerializer.Deserialize<EfiPayCobrancaResponse>(content);

    // Obter QR Code
    var locId = cobResponse.Loc.Id;
    var qrCodeRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/v2/loc/{locId}/qrcode");
    qrCodeRequest.Headers.Add("Authorization", $"Bearer {token}");

    var qrCodeResponse = await client.SendAsync(qrCodeRequest);
    var qrCodeData = JsonSerializer.Deserialize<EfiPayQrCodeResponse>(
        await qrCodeResponse.Content.ReadAsStringAsync()
    );

    return new RespostaPixDto
    {
        Status = cobResponse.Status,
        TransacaoId = txid,
        QrCodePix = qrCodeData.ImagemQrCode,      // Base64 da imagem
        PixCopiaECola = qrCodeData.QrCode,        // Texto para copiar
        DataProcessamento = DateTime.Now
    };
}
```

### Endpoints PIX

| Operacao          | Metodo | Endpoint                    |
|-------------------|--------|-----------------------------|
| Criar cobranca    | PUT    | `/v2/cob/{txid}`           |
| Consultar         | GET    | `/v2/cob/{txid}`           |
| Cancelar          | PATCH  | `/v2/cob/{txid}`           |
| Obter QR Code     | GET    | `/v2/loc/{locId}/qrcode`   |
| Transferir        | PUT    | `/v3/gn/pix/{idEnvio}`     |

### Status da Cobranca PIX

```csharp
public enum StatusCobrancaPixEfiPay
{
    ATIVA,                          // Aguardando pagamento
    CONCLUIDA,                      // Paga
    REMOVIDA_PELO_USUARIO_RECEBEDOR,// Cancelada pelo recebedor
    REMOVIDA_PELO_PSP,              // Cancelada pelo banco
    EXPIRADA                        // Prazo expirado (status interno)
}
```

---

## Cobranca Recorrente (Assinaturas)

**IDEAL PARA CONTROLE DE LICENCA DE SOFTWARE**

A API de Assinaturas permite cobrar automaticamente em intervalos definidos (mensal, trimestral, semestral, anual).

### Fluxo de Assinaturas

```
┌──────────────────────────────────────────────────────────────────┐
│  1. CRIAR PLANO                                                  │
│     POST /v1/plan                                                │
│     Define: nome, intervalo, repeticoes, valor                   │
└──────────────────────┬───────────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│  2. CRIAR ASSINATURA                                             │
│     POST /v1/plan/{id}/subscription/one-step                     │
│     Vincula cliente ao plano                                     │
└──────────────────────┬───────────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│  3. DEFINIR PAGAMENTO                                            │
│     POST /v1/subscription/{id}/pay                               │
│     Configura: cartao de credito ou boleto                       │
└──────────────────────┬───────────────────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────────────────┐
│  4. COBRANCAS AUTOMATICAS                                        │
│     Sistema cobra automaticamente conforme o plano               │
│     Webhooks notificam status dos pagamentos                     │
└──────────────────────────────────────────────────────────────────┘
```

### Criar Plano de Assinatura

```csharp
public async Task<PlanoResponse> CriarPlanoAsync(CriarPlanoRequest request)
{
    var token = await ObterTokenAcessoAsync();

    var body = new
    {
        name = request.Nome,           // "Licenca Mensal Premium"
        interval = request.Intervalo,  // 1 = mensal, 3 = trimestral, 12 = anual
        repeats = request.Repeticoes   // null = infinito, ou numero de cobrancas
    };

    var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrlCobrancas}/v1/plan")
    {
        Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
    };
    httpRequest.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(httpRequest);
    response.EnsureSuccessStatusCode();

    return JsonSerializer.Deserialize<PlanoResponse>(
        await response.Content.ReadAsStringAsync()
    );
}
```

### Criar Assinatura (One-Step)

```csharp
public async Task<AssinaturaResponse> CriarAssinaturaAsync(int planId, CriarAssinaturaRequest request)
{
    var token = await ObterTokenAcessoAsync();

    var body = new
    {
        items = new[]
        {
            new
            {
                name = request.NomeItem,     // "Licenca Software XYZ"
                value = request.Valor,       // 9990 = R$ 99,90 (centavos)
                amount = 1
            }
        },
        payment = new
        {
            credit_card = new
            {
                customer = new
                {
                    name = request.NomeCliente,
                    cpf = request.CpfCliente,
                    email = request.EmailCliente,
                    phone_number = request.TelefoneCliente,
                    birth = request.DataNascimento  // "1990-01-15"
                },
                payment_token = request.PaymentToken,  // Token do cartao (obtido via JS)
                billing_address = new
                {
                    street = request.Endereco.Rua,
                    number = request.Endereco.Numero,
                    neighborhood = request.Endereco.Bairro,
                    zipcode = request.Endereco.Cep,
                    city = request.Endereco.Cidade,
                    state = request.Endereco.Estado
                }
            }
        }
    };

    var httpRequest = new HttpRequestMessage(
        HttpMethod.Post,
        $"{_baseUrlCobrancas}/v1/plan/{planId}/subscription/one-step"
    )
    {
        Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
    };
    httpRequest.Headers.Add("Authorization", $"Bearer {token}");

    var response = await _httpClient.SendAsync(httpRequest);
    response.EnsureSuccessStatusCode();

    return JsonSerializer.Deserialize<AssinaturaResponse>(
        await response.Content.ReadAsStringAsync()
    );
}
```

### Endpoints de Assinatura

| Operacao              | Metodo | Endpoint                                    |
|-----------------------|--------|---------------------------------------------|
| Criar plano           | POST   | `/v1/plan`                                  |
| Listar planos         | GET    | `/v1/plans`                                 |
| Detalhes do plano     | GET    | `/v1/plan/{id}`                             |
| Deletar plano         | DELETE | `/v1/plan/{id}`                             |
| Criar assinatura      | POST   | `/v1/plan/{id}/subscription/one-step`       |
| Definir pagamento     | POST   | `/v1/subscription/{id}/pay`                 |
| Detalhes assinatura   | GET    | `/v1/subscription/{id}`                     |
| Atualizar assinatura  | PUT    | `/v1/subscription/{id}`                     |
| Cancelar assinatura   | PUT    | `/v1/subscription/{id}/cancel`              |
| Retentar cobranca     | POST   | `/v1/charge/{id}/retry`                     |

### Exemplo: Sistema de Licenciamento

```csharp
public class LicenciamentoService
{
    private readonly IEfiPayAssinaturaService _efiPay;
    private readonly ILicencaRepository _licencaRepo;

    public async Task<ResultadoLicenca> AtivarLicencaAsync(AtivarLicencaRequest request)
    {
        // 1. Criar assinatura na Efi
        var assinatura = await _efiPay.CriarAssinaturaAsync(
            planId: request.PlanoId,
            new CriarAssinaturaRequest
            {
                NomeItem = $"Licenca {request.Plano}",
                Valor = request.ValorCentavos,
                NomeCliente = request.Cliente.Nome,
                CpfCliente = request.Cliente.Cpf,
                EmailCliente = request.Cliente.Email,
                PaymentToken = request.TokenCartao
            }
        );

        // 2. Salvar licenca no banco
        var licenca = new Licenca
        {
            ClienteId = request.ClienteId,
            AssinaturaEfiId = assinatura.SubscriptionId,
            PlanoId = request.PlanoId,
            Status = StatusLicenca.Ativa,
            DataInicio = DateTime.Now,
            DataProximaCobranca = DateTime.Now.AddMonths(1)
        };

        await _licencaRepo.SalvarAsync(licenca);

        return new ResultadoLicenca
        {
            Sucesso = true,
            LicencaId = licenca.Id,
            ChaveLicenca = GerarChaveLicenca(licenca)
        };
    }

    public async Task ProcessarWebhookPagamentoAsync(WebhookPagamento webhook)
    {
        var licenca = await _licencaRepo.BuscarPorAssinaturaEfiAsync(webhook.SubscriptionId);

        if (webhook.Status == "paid")
        {
            // Pagamento confirmado - renovar licenca
            licenca.Status = StatusLicenca.Ativa;
            licenca.DataProximaCobranca = licenca.DataProximaCobranca.AddMonths(1);
        }
        else if (webhook.Status == "unpaid" || webhook.Status == "canceled")
        {
            // Pagamento falhou - suspender licenca
            licenca.Status = StatusLicenca.Suspensa;
        }

        await _licencaRepo.AtualizarAsync(licenca);
    }
}
```

### Trial Days (Periodo de Teste)

```csharp
// Ao criar assinatura com cartao, pode definir dias de trial
var body = new
{
    items = new[] { /* ... */ },
    payment = new
    {
        credit_card = new
        {
            trial_days = 7,  // 7 dias gratis antes da primeira cobranca
            customer = new { /* ... */ },
            payment_token = "xxx"
        }
    }
};
```

---

## Webhooks

### Registrar Webhook

```csharp
public async Task<bool> RegistrarWebhookAsync(string webhookUrl, string chavePix)
{
    var token = await ObterTokenAcessoAsync();

    using var client = CriarHttpClientComCertificado();

    var request = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/v2/webhook/{chavePix}")
    {
        Content = new StringContent(
            JsonSerializer.Serialize(new { webhookUrl }),
            Encoding.UTF8,
            "application/json"
        )
    };

    request.Headers.Add("Authorization", $"Bearer {token}");
    request.Headers.Add("x-skip-mtls-checking", "true"); // Se nao usar mTLS no webhook

    var response = await client.SendAsync(request);
    return response.IsSuccessStatusCode;
}
```

### Processar Webhook PIX

```csharp
public async Task<bool> ProcessarWebhookRecargaPixAsync(string jsonPayload)
{
    var payload = JsonSerializer.Deserialize<EfiPayWebhookNotification>(jsonPayload);

    if (payload.Pix != null && payload.Pix.Count > 0)
    {
        var pixData = payload.Pix[0];
        var txid = pixData.Txid;

        // Consultar status atualizado
        var statusResponse = await ConsultarCobrancaPixAsync(txid);

        if (statusResponse.Status == StatusCobrancaPixEfiPay.CONCLUIDA)
        {
            // Pagamento confirmado - executar logica de negocio
            return true;
        }
    }

    return false;
}
```

### Model do Webhook

```csharp
public class EfiPayWebhookNotification
{
    [JsonPropertyName("pix")]
    public List<EfiPayPixInfo> Pix { get; set; } = new();
}

public class EfiPayPixInfo
{
    [JsonPropertyName("endToEndId")]
    public string EndToEndId { get; set; } = string.Empty;

    [JsonPropertyName("txid")]
    public string Txid { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("horario")]
    public DateTime Horario { get; set; }
}
```

---

## Models e DTOs

### Cobranca PIX - Request

```csharp
public class EfiPayCriarCobrancaRequest
{
    [JsonPropertyName("calendario")]
    public EfiPayCalendario Calendario { get; set; } = new();

    [JsonPropertyName("devedor")]
    public EfiPayDevedor? Devedor { get; set; }

    [JsonPropertyName("valor")]
    public EfiPayValor Valor { get; set; } = new();

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;

    [JsonPropertyName("solicitacaoPagador")]
    public string SolicitacaoPagador { get; set; } = string.Empty;
}

public class EfiPayCalendario
{
    [JsonPropertyName("expiracao")]
    public int Expiracao { get; set; } = 3600;
}

public class EfiPayDevedor
{
    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;
}

public class EfiPayValor
{
    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;
}
```

### Cobranca PIX - Response

```csharp
public class EfiPayCobrancaResponse
{
    [JsonPropertyName("status")]
    public StatusCobrancaPixEfiPay Status { get; set; }

    [JsonPropertyName("calendario")]
    public EfiPayCalendarioResponse Calendario { get; set; } = new();

    [JsonPropertyName("txid")]
    public string Txid { get; set; } = string.Empty;

    [JsonPropertyName("loc")]
    public EfiPayLocalizacao Loc { get; set; } = new();

    [JsonPropertyName("valor")]
    public EfiPayValor Valor { get; set; } = new();

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;
}

public class EfiPayLocalizacao
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;
}

public class EfiPayQrCodeResponse
{
    [JsonPropertyName("qrcode")]
    public string QrCode { get; set; } = string.Empty;

    [JsonPropertyName("imagemQrcode")]
    public string ImagemQrCode { get; set; } = string.Empty;
}
```

---

## Referencias

### Documentacao Oficial
- [Portal de Documentacao Efi](https://dev.efipay.com.br/)
- [API de Cobrancas](https://dev.efipay.com.br/en/docs/api-cobrancas/)
- [API de Assinaturas](https://dev.efipay.com.br/en/docs/api-cobrancas/assinatura/)
- [Credenciais e Autorizacao](https://dev.efipay.com.br/en/docs/api-cobrancas/credenciais/)

### SDKs Oficiais
- [SDK .NET](https://github.com/efipay/sdk-net-apis-efi)
- [SDK PHP](https://github.com/efipay/sdk-php-apis-efi)
- [SDK Node.js](https://github.com/efipay/sdk-node-apis-efi)
- [SDK Python](https://github.com/efipay/sdk-python-apis-efi)
- [SDK Go](https://github.com/efipay/sdk-go-apis-efi)

### Contato Efi
- Site: [sejaefi.com.br](https://sejaefi.com.br)
- Suporte: Via painel da conta

---

## Dicas para Implementacao de Licenciamento

1. **Armazene o `subscription_id`** retornado pela Efi no seu banco de dados
2. **Configure webhooks** para receber notificacoes de pagamento em tempo real
3. **Implemente retry logic** para falhas de cobranca
4. **Use trial_days** para oferecer periodo de teste
5. **Valide a licenca localmente** apos confirmar pagamento via webhook
6. **Crie endpoints** para o cliente consultar status da licenca
7. **Implemente grace period** (periodo de carencia) antes de bloquear acesso

### Exemplo de Validacao de Licenca

```csharp
public async Task<bool> ValidarLicencaAsync(string chaveCliente)
{
    var licenca = await _licencaRepo.BuscarPorChaveAsync(chaveCliente);

    if (licenca == null)
        return false;

    if (licenca.Status != StatusLicenca.Ativa)
        return false;

    // Verificar se esta dentro do periodo pago
    if (DateTime.Now > licenca.DataProximaCobranca.AddDays(3)) // 3 dias de carencia
        return false;

    return true;
}
```

---

**Ultima atualizacao:** Dezembro 2025
