using api.cliente.Repositories;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Data.Repository;

namespace api.coleta.Services
{
    /// <summary>
    /// Service para gerenciamento de assinaturas.
    ///
    /// IMPORTANTE: Os métodos de criação de assinatura com pagamento (PIX, Boleto, Cartão)
    /// e gestão de planos foram migrados para o Gateway de Pagamentos Python (porta 8001).
    /// Este service agora gerencia apenas assinaturas locais e consultas.
    ///
    /// Endpoints do Gateway:
    /// - POST /api/v1/assinaturas/pix - Criar assinatura com PIX
    /// - GET /api/v1/planos - Listar planos
    /// - POST /api/v1/subscriptions/check - Verificar licença
    /// </summary>
    public class AssinaturaService : ServiceBase
    {
        private readonly AssinaturaRepository _assinaturaRepo;
        private readonly ClienteRepository _clienteRepo;
        private readonly UsuarioRepository _usuarioRepo;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly IGatewayService _gatewayService;
        private readonly INotificador _notificador;
        private readonly ILogger<AssinaturaService> _logger;

        public AssinaturaService(
            AssinaturaRepository assinaturaRepo,
            ClienteRepository clienteRepo,
            UsuarioRepository usuarioRepo,
            HistoricoPagamentoRepository pagamentoRepo,
            IGatewayService gatewayService,
            INotificador notificador,
            ILogger<AssinaturaService> logger,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _assinaturaRepo = assinaturaRepo;
            _clienteRepo = clienteRepo;
            _usuarioRepo = usuarioRepo;
            _pagamentoRepo = pagamentoRepo;
            _gatewayService = gatewayService;
            _notificador = notificador;
            _logger = logger;
        }

        public async Task<AssinaturaDTO?> AtivarAssinaturaManualAsync(Guid assinaturaId, string? observacao = null)
        {
            var assinatura = await _assinaturaRepo.ObterPorIdAsync(assinaturaId);
            if (assinatura == null)
            {
                _notificador.Notificar(new Notificacao("Assinatura não encontrada."));
                return null;
            }

            assinatura.Ativa = true;
            assinatura.StatusPagamento = "active";
            assinatura.DataUltimoPagamento = DateTime.Now;
            assinatura.Observacao = observacao ?? "Ativação manual";

            // Corrigir datas inválidas (0001-01-01)
            if (assinatura.DataInicio == default || assinatura.DataInicio.Year < 2000)
            {
                assinatura.DataInicio = DateTime.Now;
            }
            if (assinatura.DataFim == default || assinatura.DataFim.Year < 2000)
            {
                assinatura.DataFim = assinatura.DataInicio.AddYears(1);
            }

            _assinaturaRepo.Atualizar(assinatura);
            UnitOfWork.Commit();

            return MapToDTO(assinatura);
        }

        public async Task<AssinaturaDTO?> CancelarAssinaturaAsync(Guid assinaturaId)
        {
            var assinatura = await _assinaturaRepo.ObterPorIdAsync(assinaturaId);
            if (assinatura == null)
            {
                _notificador.Notificar(new Notificacao("Assinatura não encontrada."));
                return null;
            }

            assinatura.Ativa = false;
            assinatura.StatusPagamento = "canceled";

            _assinaturaRepo.Atualizar(assinatura);
            UnitOfWork.Commit();

            return MapToDTO(assinatura);
        }

        public async Task<List<AssinaturaDTO>> ListarAssinaturasDoClienteAsync(Guid clienteId)
        {
            var assinaturas = await _assinaturaRepo.ObterAssinaturasDoClienteAsync(clienteId);
            return assinaturas.Select(MapToDTO).ToList();
        }

        public async Task<AssinaturaDTO?> ObterAssinaturaAtivaDoClienteAsync(Guid clienteId)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);
            return assinatura != null ? MapToDTO(assinatura) : null;
        }

        /// <summary>
        /// Obtém assinatura ativa diretamente pelo UsuarioId
        /// </summary>
        public async Task<AssinaturaDTO?> ObterAssinaturaAtivaDoUsuarioAsync(Guid usuarioId)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoUsuarioAsync(usuarioId);
            return assinatura != null ? MapToDTO(assinatura) : null;
        }

        /// <summary>
        /// Lista histórico de pagamentos de um cliente
        /// </summary>
        public async Task<List<HistoricoPagamentoDTO>> ListarHistoricoPagamentosDoClienteAsync(Guid clienteId)
        {
            var assinaturas = await _assinaturaRepo.ObterAssinaturasDoClienteAsync(clienteId);
            var historico = new List<HistoricoPagamentoDTO>();

            foreach (var assinatura in assinaturas)
            {
                var pagamentos = await _pagamentoRepo.ObterPagamentosDaAssinaturaAsync(assinatura.Id);

                foreach (var pagamento in pagamentos)
                {
                    historico.Add(new HistoricoPagamentoDTO
                    {
                        Id = pagamento.Id,
                        AssinaturaId = pagamento.AssinaturaId,
                        PlanoId = assinatura.PlanoId,
                        Valor = pagamento.Valor,
                        DataPagamento = pagamento.DataPagamento,
                        MetodoPagamento = pagamento.MetodoPagamento,
                        Status = pagamento.Status.ToString(),
                        EfiPayStatus = pagamento.EfiPayStatus,
                        PixTxId = pagamento.PixTxId,
                        CartaoParcelas = pagamento.CartaoParcelas,
                        CartaoBandeira = pagamento.CartaoBandeira
                    });
                }
            }

            return historico.OrderByDescending(h => h.DataPagamento).ToList();
        }

        /// <summary>
        /// Lista historico de pagamentos diretamente pelo UsuarioId
        /// </summary>
        public async Task<List<HistoricoPagamentoDTO>> ListarHistoricoPagamentosDoUsuarioAsync(Guid usuarioId)
        {
            var assinaturas = await _assinaturaRepo.ObterAssinaturasDoUsuarioAsync(usuarioId);
            var historico = new List<HistoricoPagamentoDTO>();

            foreach (var assinatura in assinaturas)
            {
                var pagamentos = await _pagamentoRepo.ObterPagamentosDaAssinaturaAsync(assinatura.Id);

                foreach (var pagamento in pagamentos)
                {
                    historico.Add(new HistoricoPagamentoDTO
                    {
                        Id = pagamento.Id,
                        AssinaturaId = pagamento.AssinaturaId,
                        PlanoId = assinatura.PlanoId,
                        Valor = pagamento.Valor,
                        DataPagamento = pagamento.DataPagamento,
                        MetodoPagamento = pagamento.MetodoPagamento,
                        Status = pagamento.Status.ToString(),
                        EfiPayStatus = pagamento.EfiPayStatus,
                        PixTxId = pagamento.PixTxId,
                        CartaoParcelas = pagamento.CartaoParcelas,
                        CartaoBandeira = pagamento.CartaoBandeira
                    });
                }
            }

            return historico.OrderByDescending(h => h.DataPagamento).ToList();
        }

        /// <summary>
        /// Verifica status de pagamento PIX de uma assinatura.
        /// Tenta primeiro o gateway (PostgreSQL), depois fallback para banco local (MySQL).
        /// </summary>
        public async Task<VerificacaoPagamentoDTO?> VerificarPagamentoPixAsync(Guid assinaturaId)
        {
            // Tentar primeiro via Gateway de Pagamentos
            var gatewayResponse = await _gatewayService.VerificarPagamentoAsync(assinaturaId);
            if (gatewayResponse != null)
            {
                _logger.LogInformation("Verificacao de pagamento via gateway: AssinaturaId={AssinaturaId}, Pago={Pago}",
                    assinaturaId, gatewayResponse.Pago);

                return new VerificacaoPagamentoDTO
                {
                    AssinaturaId = gatewayResponse.AssinaturaId,
                    Status = gatewayResponse.Status,
                    Pago = gatewayResponse.Pago,
                    AssinaturaAtiva = gatewayResponse.AssinaturaAtiva,
                    Valor = gatewayResponse.Valor,
                    DataVerificacao = gatewayResponse.DataVerificacao
                };
            }

            // Fallback: buscar no banco local
            _logger.LogInformation("Gateway nao encontrou assinatura, tentando banco local: AssinaturaId={AssinaturaId}", assinaturaId);

            var assinatura = await _assinaturaRepo.ObterPorIdAsync(assinaturaId);
            if (assinatura == null)
            {
                _notificador.Notificar(new Notificacao("Assinatura não encontrada."));
                return null;
            }

            // Buscar pagamento PIX pendente
            var pagamentos = await _pagamentoRepo.ObterPagamentosDaAssinaturaAsync(assinaturaId);
            var pagamentoPix = pagamentos.FirstOrDefault(p =>
                p.MetodoPagamento == "PIX" &&
                !string.IsNullOrEmpty(p.PixTxId));

            if (pagamentoPix == null)
            {
                _notificador.Notificar(new Notificacao("Nenhum pagamento PIX encontrado para esta assinatura."));
                return null;
            }

            return new VerificacaoPagamentoDTO
            {
                AssinaturaId = assinaturaId,
                Status = pagamentoPix.Status.ToString(),
                Pago = pagamentoPix.Status == StatusPagamento.Aprovado,
                AssinaturaAtiva = assinatura.Ativa,
                Valor = pagamentoPix.Valor,
                DataVerificacao = DateTime.Now
            };
        }

        /// <summary>
        /// Deleta todas as assinaturas e pagamentos de um cliente (desenvolvimento)
        /// </summary>
        public async Task<int> DeletarTodasAssinaturasDoClienteAsync(Guid clienteId)
        {
            var assinaturas = await _assinaturaRepo.ObterAssinaturasDoClienteAsync(clienteId);
            var count = 0;

            foreach (var assinatura in assinaturas)
            {
                // Deletar pagamentos associados
                var pagamentos = await _pagamentoRepo.ObterPagamentosDaAssinaturaAsync(assinatura.Id);
                foreach (var pagamento in pagamentos)
                {
                    _pagamentoRepo.Deletar(pagamento);
                }

                // Deletar assinatura
                _assinaturaRepo.Deletar(assinatura);
                count++;
            }

            UnitOfWork.Commit();
            _logger.LogInformation("Deletadas {Count} assinaturas do cliente {ClienteId}", count, clienteId);

            return count;
        }

        /// <summary>
        /// Ativa assinatura após confirmação de pagamento via webhook do gateway
        /// </summary>
        public async Task<bool> AtivarPorWebhookAsync(Guid assinaturaId, string pagamentoId)
        {
            var assinatura = await _assinaturaRepo.ObterPorIdAsync(assinaturaId);
            if (assinatura == null)
            {
                _logger.LogWarning("Assinatura {AssinaturaId} não encontrada para ativação via webhook", assinaturaId);
                return false;
            }

            assinatura.Ativa = true;
            assinatura.StatusPagamento = "active";
            assinatura.DataUltimoPagamento = DateTime.Now;

            // Corrigir datas inválidas
            if (assinatura.DataInicio == default || assinatura.DataInicio.Year < 2000)
            {
                assinatura.DataInicio = DateTime.Now;
            }
            if (assinatura.DataFim == default || assinatura.DataFim.Year < 2000)
            {
                assinatura.DataFim = assinatura.DataInicio.AddYears(1);
            }

            _assinaturaRepo.Atualizar(assinatura);
            UnitOfWork.Commit();

            _logger.LogInformation("Assinatura {AssinaturaId} ativada via webhook, pagamento: {PagamentoId}",
                assinaturaId, pagamentoId);

            return true;
        }

        private static AssinaturaDTO MapToDTO(Assinatura a)
        {
            var diasRestantes = a.EstaVigente() ? (a.DataFim - DateTime.Now).Days : 0;

            return new AssinaturaDTO
            {
                Id = a.Id,
                ClienteId = a.ClienteId,
                ClienteNome = a.Cliente?.Nome ?? "",
                UsuarioId = a.UsuarioId,
                UsuarioNome = a.Usuario?.NomeCompleto ?? "",
                PlanoId = a.PlanoId,
                // Plano details are fetched from Gateway PostgreSQL via GatewayService
                Plano = null,
                DataInicio = a.DataInicio,
                DataFim = a.DataFim,
                Ativa = a.Ativa,
                AutoRenovar = a.AutoRenovar,
                Observacao = a.Observacao,
                EstaVigente = a.EstaVigente(),
                DiasRestantes = diasRestantes,
                StatusPagamento = a.StatusPagamento
            };
        }
    }
}
