using api.coleta.Data;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.EfiPay;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly IEfiPayService _efiPayService;
        private readonly AssinaturaRepository _assinaturaRepo;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookController> _logger;

        // IP oficial da Efi Pay para webhooks (skip-mTLS validation)
        // Ref: https://dev.efipay.com.br/docs/api-pix/webhooks
        private static readonly HashSet<string> EfiPayAllowedIPs = new()
        {
            "34.193.116.226",  // IP principal Efi Pay
            "::1",             // localhost IPv6 (dev)
            "127.0.0.1"        // localhost IPv4 (dev)
        };

        public WebhookController(
            IEfiPayService efiPayService,
            AssinaturaRepository assinaturaRepo,
            HistoricoPagamentoRepository pagamentoRepo,
            IUnitOfWork unitOfWork,
            ILogger<WebhookController> logger)
        {
            _efiPayService = efiPayService;
            _assinaturaRepo = assinaturaRepo;
            _pagamentoRepo = pagamentoRepo;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Valida se o IP da requisição é da Efi Pay (skip-mTLS security)
        /// </summary>
        private bool ValidarIPEfiPay()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Verificar header X-Forwarded-For (proxy/load balancer)
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // Pegar o primeiro IP (origem real)
                remoteIp = forwardedFor.Split(',')[0].Trim();
            }

            _logger.LogInformation("Webhook request from IP: {RemoteIp}, X-Forwarded-For: {ForwardedFor}",
                HttpContext.Connection.RemoteIpAddress, forwardedFor);

            if (string.IsNullOrEmpty(remoteIp))
            {
                _logger.LogWarning("Could not determine remote IP for webhook request");
                return false;
            }

            // Em produção, validar IP da Efi Pay
            var isValid = EfiPayAllowedIPs.Contains(remoteIp);

            if (!isValid)
            {
                _logger.LogWarning("Webhook request from unauthorized IP: {RemoteIp}", remoteIp);
            }

            return isValid;
        }

        /// <summary>
        /// Valida hash HMAC na query string para autenticação adicional (skip-mTLS)
        /// URL cadastrada deve ter: ?hmac=SUA_HASH_SECRETA
        /// </summary>
        private bool ValidarHmacWebhook()
        {
            // Hash secreta configurada (deve ser a mesma cadastrada na Efi Pay)
            const string WEBHOOK_HMAC_SECRET = "agrosyste_webhook_2024_secret";

            var hmacQuery = HttpContext.Request.Query["hmac"].FirstOrDefault();

            if (string.IsNullOrEmpty(hmacQuery))
            {
                // Se não tem hmac na query, aceitar (para compatibilidade)
                // Em produção, pode-se tornar obrigatório
                _logger.LogDebug("No HMAC in query string - skipping validation");
                return true;
            }

            var isValid = hmacQuery == WEBHOOK_HMAC_SECRET;

            if (!isValid)
            {
                _logger.LogWarning("Invalid HMAC in webhook request: {Hmac}", hmacQuery);
            }

            return isValid;
        }

        /// <summary>
        /// Webhook endpoint for EfiPay PIX notifications
        /// URL: https://apis-api-coleta.w4dxlp.easypanel.host/api/webhook/efipay
        /// Efi Pay adiciona /pix ao final automaticamente
        /// </summary>
        [HttpPost("efipay")]
        public async Task<IActionResult> ReceberNotificacaoEfiPay([FromBody] EfiPayWebhookNotificationDTO notification)
        {
            try
            {
                _logger.LogInformation("Received EfiPay PIX webhook notification");

                // Validar origem da requisição (skip-mTLS security)
                if (!ValidarIPEfiPay())
                {
                    _logger.LogWarning("Webhook request rejected - invalid IP");
                    // Retornar 200 para não expor informações de segurança
                    return Ok();
                }

                if (!ValidarHmacWebhook())
                {
                    _logger.LogWarning("Webhook request rejected - invalid HMAC");
                    return Ok();
                }

                if (notification.Pix != null && notification.Pix.Count > 0)
                {
                    foreach (var pixData in notification.Pix)
                    {
                        _logger.LogInformation("Processing PIX payment: txid={Txid}, valor={Valor}",
                            pixData.Txid, pixData.Valor);

                        await _efiPayService.ProcessarWebhookPixAsync(pixData);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing EfiPay webhook");
                // Return 200 to prevent EfiPay from retrying
                return Ok();
            }
        }

        /// <summary>
        /// Webhook unificado para notificacoes da API de Cobrancas EfiPay
        /// Processa: Boleto, Cartao e Assinatura Recorrente (renovação automática)
        /// URL: https://apis-api-coleta.w4dxlp.easypanel.host/api/webhook/efipay/cobranca
        /// </summary>
        [HttpPost("efipay/cobranca")]
        public async Task<IActionResult> ReceberNotificacaoCobranca([FromBody] JsonElement payload)
        {
            try
            {
                var json = payload.GetRawText();
                _logger.LogInformation("Received EfiPay Cobranca webhook: {Payload}", json);

                // Validar origem da requisição (skip-mTLS security)
                if (!ValidarIPEfiPay())
                {
                    _logger.LogWarning("Cobranca webhook request rejected - invalid IP");
                    return Ok();
                }

                if (!ValidarHmacWebhook())
                {
                    _logger.LogWarning("Cobranca webhook request rejected - invalid HMAC");
                    return Ok();
                }

                // Identificar tipo de notificacao
                if (payload.TryGetProperty("id", out var idProperty) &&
                    payload.TryGetProperty("status", out var statusProperty))
                {
                    var chargeId = idProperty.GetInt64();
                    var status = statusProperty.GetString();
                    var identifiers = payload.TryGetProperty("identifiers", out var identsProperty)
                        ? identsProperty
                        : default;

                    // Verificar se e uma assinatura recorrente (RENOVAÇÃO AUTOMÁTICA)
                    if (payload.TryGetProperty("subscription_id", out var subscriptionIdProperty))
                    {
                        _logger.LogInformation("Processing RECURRING subscription payment: subscriptionId={SubscriptionId}",
                            subscriptionIdProperty.GetInt64());

                        await ProcessarNotificacaoAssinaturaRecorrente(
                            subscriptionIdProperty.GetInt64(),
                            chargeId,
                            status ?? "unknown"
                        );
                    }
                    // Ou uma cobranca avulsa (boleto ou cartao)
                    else
                    {
                        await ProcessarNotificacaoCobranca(chargeId, status ?? "unknown", identifiers);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing EfiPay Cobranca webhook");
                return Ok();
            }
        }

        private async Task ProcessarNotificacaoCobranca(long chargeId, string status, JsonElement identifiers)
        {
            _logger.LogInformation("Processing charge notification: chargeId={ChargeId}, status={Status}",
                chargeId, status);

            // Buscar pagamento pelo ChargeId
            var pagamento = await _pagamentoRepo.ObterPorEfiPayChargeIdAsync(chargeId.ToString());
            if (pagamento == null)
            {
                _logger.LogWarning("Payment not found for chargeId: {ChargeId}", chargeId);
                return;
            }

            // Mapear status EfiPay para StatusPagamento enum
            var statusMapeado = MapearStatusCobrancaParaEnum(status);

            // Atualizar status do pagamento
            pagamento.Status = statusMapeado;
            pagamento.EfiPayStatus = status;

            if (statusMapeado == StatusPagamento.Aprovado)
            {
                pagamento.DataPagamento = DateTime.Now;

                // Ativar assinatura
                var assinatura = await _assinaturaRepo.ObterPorIdAsync(pagamento.AssinaturaId);
                if (assinatura != null && !assinatura.Ativa)
                {
                    assinatura.Ativa = true;
                    assinatura.DataUltimoPagamento = DateTime.Now;
                    _assinaturaRepo.Atualizar(assinatura);
                    _logger.LogInformation("Subscription activated: {AssinaturaId}", assinatura.Id);
                }
            }

            _pagamentoRepo.Atualizar(pagamento);
            _unitOfWork.Commit();

            _logger.LogInformation("Payment updated: chargeId={ChargeId}, newStatus={Status}",
                chargeId, statusMapeado);
        }

        private async Task ProcessarNotificacaoAssinaturaRecorrente(long subscriptionId, long chargeId, string status)
        {
            _logger.LogInformation("Processing subscription notification: subscriptionId={SubscriptionId}, chargeId={ChargeId}, status={Status}",
                subscriptionId, chargeId, status);

            // Buscar assinatura pelo EfiPaySubscriptionId
            var assinatura = await _assinaturaRepo.ObterPorEfiPaySubscriptionIdAsync(subscriptionId.ToString());
            if (assinatura == null)
            {
                _logger.LogWarning("Subscription not found for subscriptionId: {SubscriptionId}", subscriptionId);
                return;
            }

            var statusMapeado = MapearStatusCobrancaParaEnum(status);

            if (statusMapeado == StatusPagamento.Aprovado)
            {
                // Atualizar data do ultimo pagamento
                assinatura.DataUltimoPagamento = DateTime.Now;

                // Garantir que esta ativa
                if (!assinatura.Ativa)
                {
                    assinatura.Ativa = true;
                }

                // Registrar pagamento mensal
                var pagamento = new HistoricoPagamento
                {
                    AssinaturaId = assinatura.Id,
                    Valor = assinatura.Plano?.ValorMensal ?? 0,
                    MetodoPagamento = "recorrente",
                    Status = StatusPagamento.Aprovado,
                    EfiPayChargeId = chargeId.ToString(),
                    EfiPayStatus = status,
                    DataPagamento = DateTime.Now
                };

                _pagamentoRepo.Adicionar(pagamento);
                _assinaturaRepo.Atualizar(assinatura);

                _logger.LogInformation("Recurring payment registered for subscription: {AssinaturaId}", assinatura.Id);
            }
            else if (status == "canceled" || status == "unpaid")
            {
                // Marcar como inativa se pagamento falhou
                assinatura.Ativa = false;
                _assinaturaRepo.Atualizar(assinatura);

                _logger.LogWarning("Subscription deactivated due to payment failure: {AssinaturaId}", assinatura.Id);
            }

            _unitOfWork.Commit();
        }

        private StatusPagamento MapearStatusCobrancaParaEnum(string statusEfiPay)
        {
            return statusEfiPay?.ToLower() switch
            {
                "paid" => StatusPagamento.Aprovado,
                "settled" => StatusPagamento.Aprovado,
                "waiting" => StatusPagamento.Pendente,
                "unpaid" => StatusPagamento.Recusado,
                "canceled" => StatusPagamento.Cancelado,
                "refunded" => StatusPagamento.Estornado,
                "contested" => StatusPagamento.Recusado,
                "expired" => StatusPagamento.Cancelado,
                _ => StatusPagamento.Pendente
            };
        }

        /// <summary>
        /// Test endpoint to verify webhook is reachable
        /// </summary>
        [HttpGet("efipay/test")]
        public IActionResult TestWebhook()
        {
            return Ok(new
            {
                status = "ok",
                message = "Webhook endpoint is reachable",
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Endpoint para configuracao do webhook PIX (chamado pelo EfiPay)
        /// </summary>
        [HttpPost("efipay/pix")]
        public async Task<IActionResult> ReceberNotificacaoPixEfiPay([FromBody] EfiPayWebhookNotificationDTO notification)
        {
            // Same as efipay endpoint, just different route
            return await ReceberNotificacaoEfiPay(notification);
        }

        // ===== WEBHOOK MANAGEMENT ENDPOINTS =====

        /// <summary>
        /// Cadastra webhook PIX na Efi Pay (skip-mTLS)
        /// Usa a URL configurada no sistema ou a passada como parâmetro
        /// </summary>
        [HttpPost("efipay/configurar")]
        public async Task<IActionResult> ConfigurarWebhookEfiPay([FromBody] ConfigurarWebhookRequest? request = null)
        {
            try
            {
                _logger.LogInformation("Configurando webhook PIX na Efi Pay...");

                var resultado = await _efiPayService.CadastrarWebhookPixAsync(request?.WebhookUrl);

                if (resultado.Sucesso)
                {
                    return Ok(resultado);
                }

                return BadRequest(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar webhook PIX");
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Consulta webhook PIX configurado na Efi Pay
        /// </summary>
        [HttpGet("efipay/configuracao")]
        public async Task<IActionResult> ConsultarWebhookEfiPay()
        {
            try
            {
                var webhook = await _efiPayService.ConsultarWebhookPixAsync();

                if (webhook == null)
                {
                    return Ok(new
                    {
                        configurado = false,
                        mensagem = "Webhook PIX não está configurado na Efi Pay"
                    });
                }

                return Ok(new
                {
                    configurado = true,
                    webhookUrl = webhook.WebhookUrl,
                    chavePix = webhook.Chave,
                    dataCriacao = webhook.Criacao
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar webhook PIX");
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Remove webhook PIX da Efi Pay
        /// </summary>
        [HttpDelete("efipay/configuracao")]
        public async Task<IActionResult> RemoverWebhookEfiPay()
        {
            try
            {
                var sucesso = await _efiPayService.RemoverWebhookPixAsync();

                if (sucesso)
                {
                    return Ok(new { mensagem = "Webhook PIX removido com sucesso" });
                }

                return BadRequest(new { erro = "Falha ao remover webhook PIX" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover webhook PIX");
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request para configurar webhook
    /// </summary>
    public class ConfigurarWebhookRequest
    {
        /// <summary>
        /// URL do webhook (opcional, usa a configurada no sistema se não informada)
        /// </summary>
        public string? WebhookUrl { get; set; }
    }
}
