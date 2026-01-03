using api.cliente.Repositories;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.EfiPay;
using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Data.Repository;

namespace api.coleta.Services
{
    public class AssinaturaService : ServiceBase
    {
        private readonly AssinaturaRepository _assinaturaRepo;
        private readonly PlanoRepository _planoRepo;
        private readonly ClienteRepository _clienteRepo;
        private readonly UsuarioRepository _usuarioRepo;
        private readonly HistoricoPagamentoRepository _pagamentoRepo;
        private readonly IEfiPayService _efiPayService;
        private readonly INotificador _notificador;
        private readonly ILogger<AssinaturaService> _logger;

        public AssinaturaService(
            AssinaturaRepository assinaturaRepo,
            PlanoRepository planoRepo,
            ClienteRepository clienteRepo,
            UsuarioRepository usuarioRepo,
            HistoricoPagamentoRepository pagamentoRepo,
            IEfiPayService efiPayService,
            INotificador notificador,
            ILogger<AssinaturaService> logger,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _assinaturaRepo = assinaturaRepo;
            _planoRepo = planoRepo;
            _clienteRepo = clienteRepo;
            _usuarioRepo = usuarioRepo;
            _pagamentoRepo = pagamentoRepo;
            _efiPayService = efiPayService;
            _notificador = notificador;
            _logger = logger;
        }

        public async Task<AssinaturaComPagamentoDTO?> CriarAssinaturaComPagamentoPixAsync(
            AssinaturaCreateComPixDTO dto)
        {
            // Validate ClienteId is provided
            if (!dto.ClienteId.HasValue || dto.ClienteId.Value == Guid.Empty)
            {
                _notificador.Notificar(new Notificacao("ClienteId é obrigatório para esta operação."));
                return null;
            }

            // Validate Cliente
            var cliente = _clienteRepo.ObterPorId(dto.ClienteId.Value);
            if (cliente == null)
            {
                _notificador.Notificar(new Notificacao("Cliente não encontrado."));
                return null;
            }

            // Validate Plano
            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return null;
            }

            // Gold plan requires contact
            if (plano.RequereContato)
            {
                _notificador.Notificar(new Notificacao(
                    "Plano Gold requer contato direto. Entre em contato conosco para um orçamento personalizado."
                ));
                return null;
            }

            // Deactivate existing subscriptions
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(dto.ClienteId.Value);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por nova assinatura em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            // Create subscription (inactive until payment confirmed)
            // Se as datas não foram fornecidas, usar valores padrão (1 ano a partir de agora)
            var dataInicio = dto.DataInicio == default ? DateTime.Now : dto.DataInicio;
            var dataFim = dto.DataFim == default ? dataInicio.AddYears(1) : dto.DataFim;

            var assinatura = new Assinatura
            {
                ClienteId = dto.ClienteId.Value,
                PlanoId = dto.PlanoId,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Ativa = false, // Will be activated by webhook
                AutoRenovar = dto.AutoRenovar,
                StatusPagamento = "pending",
                Observacao = dto.Observacao
            };

            _assinaturaRepo.Adicionar(assinatura);
            UnitOfWork.Commit();

            // Load plano for response
            assinatura.Plano = plano;
            assinatura.Cliente = cliente;

            // Create PIX charge via EfiPay
            try
            {
                if (!_efiPayService.EstaConfigurado())
                {
                    _logger.LogWarning("EfiPay not configured, creating subscription without payment");

                    // Return without PIX payment info
                    return new AssinaturaComPagamentoDTO
                    {
                        Assinatura = MapToDTO(assinatura),
                        Pagamento = null
                    };
                }

                var pixResponse = await _efiPayService.CriarCobrancaPixAsync(
                    plano.ValorAnual,
                    dto.CpfCnpj,
                    cliente.Nome,
                    $"AgroSyste - {plano.Nome}"
                );

                // Get QR Code
                var qrCode = await _efiPayService.ObterQrCodePixAsync(pixResponse.Loc!.Id);

                // Create payment record
                var pagamento = new HistoricoPagamento
                {
                    AssinaturaId = assinatura.Id,
                    Valor = plano.ValorAnual,
                    DataPagamento = DateTime.Now,
                    MetodoPagamento = "PIX",
                    Status = StatusPagamento.Pendente,
                    PixTxId = pixResponse.Txid,
                    PixQrCode = qrCode.Qrcode,
                    PixQrCodeBase64 = qrCode.ImagemQrcode,
                    DataExpiracao = DateTime.Now.AddHours(24),
                    EfiPayChargeId = pixResponse.Loc.Id.ToString(),
                    EfiPayStatus = pixResponse.Status
                };

                _pagamentoRepo.Adicionar(pagamento);
                UnitOfWork.Commit();

                _logger.LogInformation("PIX charge created for subscription {AssinaturaId}, txid={Txid}",
                    assinatura.Id, pixResponse.Txid);

                return new AssinaturaComPagamentoDTO
                {
                    Assinatura = MapToDTO(assinatura),
                    Pagamento = new PagamentoPixDTO
                    {
                        TxId = pixResponse.Txid,
                        QrCode = qrCode.Qrcode,
                        QrCodeImagem = qrCode.ImagemQrcode,
                        Valor = plano.ValorAnual,
                        DataExpiracao = DateTime.Now.AddHours(24)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cobrança PIX para assinatura {AssinaturaId}", assinatura.Id);
                _notificador.Notificar(new Notificacao($"Erro ao gerar PIX: {ex.Message}"));
                return null;
            }
        }

        // Método para criar assinatura vinculada ao USUÁRIO (não cliente)
        public async Task<AssinaturaComPagamentoDTO?> CriarAssinaturaUsuarioComPixAsync(
            Guid usuarioId, AssinaturaCreateUsuarioDTO dto)
        {
            // Validate Usuario
            var usuario = _usuarioRepo.ObterPorId(usuarioId);
            if (usuario == null)
            {
                _notificador.Notificar(new Notificacao("Usuário não encontrado."));
                return null;
            }

            // Validate Plano
            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return null;
            }

            // Gold plan requires contact
            if (plano.RequereContato)
            {
                _notificador.Notificar(new Notificacao(
                    "Plano Gold requer contato direto. Entre em contato conosco para um orçamento personalizado."
                ));
                return null;
            }

            // Deactivate existing subscriptions (by usuario)
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaPorUsuarioAsync(usuarioId);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por nova assinatura em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            // Create subscription linked to Usuario
            var assinatura = new Assinatura
            {
                UsuarioId = usuarioId,  // Vincula ao usuario, não ao cliente
                ClienteId = null,
                PlanoId = dto.PlanoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddYears(1),
                Ativa = false, // Will be activated by webhook
                AutoRenovar = false,
                StatusPagamento = "pending"
            };

            _assinaturaRepo.Adicionar(assinatura);
            UnitOfWork.Commit();

            // Create PIX charge using user's data
            try
            {
                var cpfCnpj = dto.CpfCnpj ?? usuario.CPF ?? "";
                var pixResponse = await _efiPayService.CriarCobrancaPixAsync(
                    plano.ValorAnual,
                    cpfCnpj,
                    usuario.NomeCompleto ?? "Usuário",
                    $"AgroSyste - {plano.Nome}"
                );

                // Get QR Code
                var qrCode = await _efiPayService.ObterQrCodePixAsync(pixResponse.Loc!.Id);

                // Create payment record
                var pagamento = new HistoricoPagamento
                {
                    AssinaturaId = assinatura.Id,
                    Valor = plano.ValorAnual,
                    DataPagamento = DateTime.Now,
                    MetodoPagamento = "PIX",
                    Status = StatusPagamento.Pendente,
                    PixTxId = pixResponse.Txid,
                    PixQrCode = qrCode.Qrcode,
                    PixQrCodeBase64 = qrCode.ImagemQrcode,
                    DataExpiracao = DateTime.Now.AddHours(24),
                    EfiPayChargeId = pixResponse.Loc.Id.ToString(),
                    EfiPayStatus = pixResponse.Status
                };

                _pagamentoRepo.Adicionar(pagamento);
                UnitOfWork.Commit();

                _logger.LogInformation("PIX charge created for user subscription {AssinaturaId}, txid={Txid}, usuarioId={UsuarioId}",
                    assinatura.Id, pixResponse.Txid, usuarioId);

                return new AssinaturaComPagamentoDTO
                {
                    Assinatura = MapToDTO(assinatura),
                    Pagamento = new PagamentoPixDTO
                    {
                        TxId = pixResponse.Txid,
                        QrCode = qrCode.Qrcode,
                        QrCodeImagem = qrCode.ImagemQrcode,
                        Valor = plano.ValorAnual,
                        DataExpiracao = DateTime.Now.AddHours(24)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cobrança PIX para assinatura de usuario {AssinaturaId}", assinatura.Id);
                _notificador.Notificar(new Notificacao($"Erro ao gerar PIX: {ex.Message}"));
                return null;
            }
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
        /// Obtém assinatura ativa diretamente pelo UsuarioId (via Cliente)
        /// </summary>
        public async Task<AssinaturaDTO?> ObterAssinaturaAtivaDoUsuarioAsync(Guid usuarioId)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoUsuarioAsync(usuarioId);
            return assinatura != null ? MapToDTO(assinatura) : null;
        }

        /// <summary>
        /// Cria assinatura pendente para pagamento via boleto
        /// </summary>
        public async Task<AssinaturaDTO?> CriarAssinaturaPendenteAsync(
            Guid planoId,
            Guid clienteId,
            string metodoPagamento,
            string chargeId)
        {
            var cliente = _clienteRepo.ObterPorId(clienteId);
            if (cliente == null)
            {
                _notificador.Notificar(new Notificacao("Cliente não encontrado."));
                return null;
            }

            var plano = await _planoRepo.ObterPorIdAsync(planoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return null;
            }

            // Desativar assinaturas existentes
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por nova assinatura em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            var assinatura = new Assinatura
            {
                ClienteId = clienteId,
                PlanoId = planoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddYears(1),
                Ativa = false,
                AutoRenovar = false,
                StatusPagamento = "pending",
                Observacao = $"Aguardando pagamento via {metodoPagamento}"
            };

            _assinaturaRepo.Adicionar(assinatura);

            var pagamento = new HistoricoPagamento
            {
                AssinaturaId = assinatura.Id,
                Valor = plano.ValorAnual,
                DataPagamento = DateTime.Now,
                MetodoPagamento = metodoPagamento.ToUpper(),
                Status = StatusPagamento.Pendente,
                EfiPayChargeId = chargeId,
                EfiPayStatus = "waiting"
            };

            _pagamentoRepo.Adicionar(pagamento);
            UnitOfWork.Commit();

            assinatura.Plano = plano;
            assinatura.Cliente = cliente;

            _logger.LogInformation("Assinatura pendente criada: {AssinaturaId}, método: {Metodo}, chargeId: {ChargeId}",
                assinatura.Id, metodoPagamento, chargeId);

            return MapToDTO(assinatura);
        }

        /// <summary>
        /// Cria assinatura ativa após pagamento aprovado via cartão
        /// </summary>
        public async Task<AssinaturaDTO?> CriarAssinaturaAtivaPorCartaoAsync(
            Guid planoId,
            Guid clienteId,
            string chargeId,
            int parcelas,
            decimal valorParcela,
            string? bandeira)
        {
            var cliente = _clienteRepo.ObterPorId(clienteId);
            if (cliente == null)
            {
                _notificador.Notificar(new Notificacao("Cliente não encontrado."));
                return null;
            }

            var plano = await _planoRepo.ObterPorIdAsync(planoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return null;
            }

            // Desativar assinaturas existentes
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por nova assinatura em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            var assinatura = new Assinatura
            {
                ClienteId = clienteId,
                PlanoId = planoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddYears(1),
                Ativa = true,
                AutoRenovar = false,
                StatusPagamento = "active",
                DataUltimoPagamento = DateTime.Now,
                Observacao = $"Pagamento via cartão em {parcelas}x"
            };

            _assinaturaRepo.Adicionar(assinatura);

            var pagamento = new HistoricoPagamento
            {
                AssinaturaId = assinatura.Id,
                Valor = plano.ValorAnual,
                DataPagamento = DateTime.Now,
                MetodoPagamento = "CARTAO",
                Status = StatusPagamento.Aprovado,
                EfiPayChargeId = chargeId,
                EfiPayStatus = "approved",
                CartaoParcelas = parcelas,
                CartaoValorParcela = valorParcela,
                CartaoBandeira = bandeira
            };

            _pagamentoRepo.Adicionar(pagamento);
            UnitOfWork.Commit();

            assinatura.Plano = plano;
            assinatura.Cliente = cliente;

            _logger.LogInformation("Assinatura ativa criada via cartão: {AssinaturaId}, {Parcelas}x de {Valor}",
                assinatura.Id, parcelas, valorParcela);

            return MapToDTO(assinatura);
        }

        /// <summary>
        /// Cria assinatura recorrente (mensal)
        /// </summary>
        public async Task<AssinaturaDTO?> CriarAssinaturaRecorrenteAsync(
            Guid planoId,
            Guid clienteId,
            string subscriptionId,
            string metodoPagamento)
        {
            var cliente = _clienteRepo.ObterPorId(clienteId);
            if (cliente == null)
            {
                _notificador.Notificar(new Notificacao("Cliente não encontrado."));
                return null;
            }

            var plano = await _planoRepo.ObterPorIdAsync(planoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return null;
            }

            // Desativar assinaturas existentes
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por assinatura recorrente em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            var assinatura = new Assinatura
            {
                ClienteId = clienteId,
                PlanoId = planoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddYears(1),
                Ativa = true,
                AutoRenovar = true,
                StatusPagamento = "active",
                DataUltimoPagamento = DateTime.Now,
                Observacao = $"Assinatura recorrente mensal via {metodoPagamento}"
            };

            _assinaturaRepo.Adicionar(assinatura);

            // Criar primeiro pagamento
            var pagamento = new HistoricoPagamento
            {
                AssinaturaId = assinatura.Id,
                Valor = plano.ValorMensal,
                DataPagamento = DateTime.Now,
                MetodoPagamento = metodoPagamento == "credit_card" ? "CARTAO_RECORRENTE" : "BOLETO_RECORRENTE",
                Status = metodoPagamento == "credit_card" ? StatusPagamento.Aprovado : StatusPagamento.Pendente,
                EfiPaySubscriptionId = subscriptionId,
                EfiPayStatus = metodoPagamento == "credit_card" ? "approved" : "waiting",
                RecorrenciaParcela = 1,
                RecorrenciaTotalParcelas = 12
            };

            _pagamentoRepo.Adicionar(pagamento);
            UnitOfWork.Commit();

            assinatura.Plano = plano;
            assinatura.Cliente = cliente;

            _logger.LogInformation("Assinatura recorrente criada: {AssinaturaId}, subscriptionId: {SubscriptionId}",
                assinatura.Id, subscriptionId);

            return MapToDTO(assinatura);
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
                        PlanoNome = assinatura.Plano?.Nome ?? "N/A",
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
                        PlanoNome = assinatura.Plano?.Nome ?? "N/A",
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
        /// Verifica status de pagamento PIX de uma assinatura
        /// </summary>
        public async Task<VerificacaoPagamentoDTO?> VerificarPagamentoPixAsync(Guid assinaturaId)
        {
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

            // Consultar status na EfiPay
            try
            {
                var pixStatus = await _efiPayService.ConsultarCobrancaPixAsync(pagamentoPix.PixTxId!);

                var isPago = pixStatus.Status?.ToLower() == "concluida" ||
                             pixStatus.Status?.ToLower() == "concluído" ||
                             pixStatus.Status?.ToLower() == "pago";

                if (isPago && pagamentoPix.Status != StatusPagamento.Aprovado)
                {
                    // Ativar assinatura
                    pagamentoPix.Status = StatusPagamento.Aprovado;
                    pagamentoPix.EfiPayStatus = pixStatus.Status;
                    pagamentoPix.DataPagamento = DateTime.Now;
                    _pagamentoRepo.Atualizar(pagamentoPix);

                    assinatura.Ativa = true;
                    assinatura.StatusPagamento = "active";
                    assinatura.DataUltimoPagamento = DateTime.Now;

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

                    _logger.LogInformation("Pagamento PIX confirmado e assinatura ativada: {AssinaturaId}", assinaturaId);
                }

                return new VerificacaoPagamentoDTO
                {
                    AssinaturaId = assinaturaId,
                    Status = pixStatus.Status ?? "unknown",
                    Pago = isPago,
                    AssinaturaAtiva = assinatura.Ativa,
                    Valor = pagamentoPix.Valor,
                    DataVerificacao = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar pagamento PIX para assinatura {AssinaturaId}", assinaturaId);
                _notificador.Notificar(new Notificacao($"Erro ao verificar pagamento: {ex.Message}"));
                return null;
            }
        }

        /// <summary>
        /// Cria assinatura vinculada ao USUÁRIO com pagamento via Boleto
        /// </summary>
        public async Task<(AssinaturaDTO? Assinatura, PagamentoBoletoDTO? Boleto)> CriarAssinaturaUsuarioComBoletoAsync(
            Guid usuarioId, AssinaturaCreateUsuarioDTO dto, PagamentoBoletoDTO boleto)
        {
            var usuario = _usuarioRepo.ObterPorId(usuarioId);
            if (usuario == null)
            {
                _notificador.Notificar(new Notificacao("Usuário não encontrado."));
                return (null, null);
            }

            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return (null, null);
            }

            // Desativar assinatura existente do usuário
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaPorUsuarioAsync(usuarioId);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por nova assinatura em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            var assinatura = new Assinatura
            {
                UsuarioId = usuarioId,
                ClienteId = null,
                PlanoId = dto.PlanoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddYears(1),
                Ativa = false, // Será ativada após confirmação do pagamento
                AutoRenovar = false,
                StatusPagamento = "pending",
                Observacao = "Aguardando pagamento via boleto"
            };

            _assinaturaRepo.Adicionar(assinatura);

            var pagamento = new HistoricoPagamento
            {
                AssinaturaId = assinatura.Id,
                Valor = plano.ValorAnual,
                DataPagamento = DateTime.Now,
                MetodoPagamento = "BOLETO",
                Status = StatusPagamento.Pendente,
                EfiPayChargeId = boleto.ChargeId.ToString(),
                EfiPayStatus = "waiting"
            };

            _pagamentoRepo.Adicionar(pagamento);
            UnitOfWork.Commit();

            _logger.LogInformation("Assinatura de usuário com boleto criada: {AssinaturaId}, chargeId: {ChargeId}, usuarioId: {UsuarioId}",
                assinatura.Id, boleto.ChargeId, usuarioId);

            return (MapToDTO(assinatura), boleto);
        }

        /// <summary>
        /// Cria assinatura vinculada ao USUÁRIO com pagamento via Cartão (ativa imediatamente)
        /// </summary>
        public async Task<(AssinaturaDTO? Assinatura, PagamentoCartaoDTO? Pagamento)> CriarAssinaturaUsuarioComCartaoAsync(
            Guid usuarioId, AssinaturaCreateUsuarioDTO dto, PagamentoCartaoDTO cartao)
        {
            var usuario = _usuarioRepo.ObterPorId(usuarioId);
            if (usuario == null)
            {
                _notificador.Notificar(new Notificacao("Usuário não encontrado."));
                return (null, null);
            }

            var plano = await _planoRepo.ObterPorIdAsync(dto.PlanoId);
            if (plano == null || !plano.Ativo)
            {
                _notificador.Notificar(new Notificacao("Plano não encontrado ou inativo."));
                return (null, null);
            }

            // Desativar assinatura existente do usuário
            var existente = await _assinaturaRepo.ObterAssinaturaAtivaPorUsuarioAsync(usuarioId);
            if (existente != null)
            {
                existente.Ativa = false;
                existente.Observacao = $"Substituída por nova assinatura em {DateTime.Now:dd/MM/yyyy}";
                _assinaturaRepo.Atualizar(existente);
            }

            var assinatura = new Assinatura
            {
                UsuarioId = usuarioId,
                ClienteId = null,
                PlanoId = dto.PlanoId,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now.AddYears(1),
                Ativa = true, // Cartão aprovado = assinatura ativa
                AutoRenovar = false,
                StatusPagamento = "active",
                DataUltimoPagamento = DateTime.Now,
                Observacao = $"Pagamento via cartão em {cartao.Parcelas}x"
            };

            _assinaturaRepo.Adicionar(assinatura);

            var pagamento = new HistoricoPagamento
            {
                AssinaturaId = assinatura.Id,
                Valor = plano.ValorAnual,
                DataPagamento = DateTime.Now,
                MetodoPagamento = "CARTAO",
                Status = StatusPagamento.Aprovado,
                EfiPayChargeId = cartao.ChargeId.ToString(),
                EfiPayStatus = "approved",
                CartaoParcelas = cartao.Parcelas,
                CartaoValorParcela = cartao.ValorParcela,
                CartaoBandeira = cartao.Bandeira
            };

            _pagamentoRepo.Adicionar(pagamento);
            UnitOfWork.Commit();

            _logger.LogInformation("Assinatura de usuário com cartão criada e ativada: {AssinaturaId}, {Parcelas}x, usuarioId: {UsuarioId}",
                assinatura.Id, cartao.Parcelas, usuarioId);

            return (MapToDTO(assinatura), cartao);
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
                Plano = a.Plano != null ? new PlanoDTO
                {
                    Id = a.Plano.Id,
                    Nome = a.Plano.Nome,
                    Descricao = a.Plano.Descricao,
                    ValorAnual = a.Plano.ValorAnual,
                    LimiteHectares = a.Plano.LimiteHectares,
                    Ativo = a.Plano.Ativo,
                    RequereContato = a.Plano.RequereContato
                } : null,
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
