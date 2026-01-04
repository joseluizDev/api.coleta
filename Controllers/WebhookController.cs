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
        /// Webhook endpoint for EfiPay PIX notifications
        /// URL: https://your-domain.com/api/webhook/efipay
        /// </summary>
        [HttpPost("efipay")]
        public async Task<IActionResult> ReceberNotificacaoEfiPay([FromBody] EfiPayWebhookNotificationDTO notification)
        {
            try
            {
                _logger.LogInformation("Received EfiPay webhook notification");

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
        /// Processa: Boleto, Cartao e Assinatura Recorrente
        /// </summary>
        [HttpPost("efipay/cobranca")]
        public async Task<IActionResult> ReceberNotificacaoCobranca([FromBody] JsonElement payload)
        {
            try
            {
                var json = payload.GetRawText();
                _logger.LogInformation("Received EfiPay Cobranca webhook: {Payload}", json);

                // Identificar tipo de notificacao
                if (payload.TryGetProperty("id", out var idProperty) &&
                    payload.TryGetProperty("status", out var statusProperty))
                {
                    var chargeId = idProperty.GetInt64();
                    var status = statusProperty.GetString();
                    var identifiers = payload.TryGetProperty("identifiers", out var identsProperty)
                        ? identsProperty
                        : default;

                    // Verificar se e uma assinatura recorrente
                    if (payload.TryGetProperty("subscription_id", out var subscriptionIdProperty))
                    {
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
    }
}
