using api.cliente.Repositories;
using api.coleta.Interfaces;
using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Data.Repository;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public AssinaturaService(
            AssinaturaRepository assinaturaRepo,
            ClienteRepository clienteRepo,
            UsuarioRepository usuarioRepo,
            HistoricoPagamentoRepository pagamentoRepo,
            IGatewayService gatewayService,
            INotificador notificador,
            ILogger<AssinaturaService> logger,
            IMemoryCache cache,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _assinaturaRepo = assinaturaRepo;
            _clienteRepo = clienteRepo;
            _usuarioRepo = usuarioRepo;
            _pagamentoRepo = pagamentoRepo;
            _gatewayService = gatewayService;
            _notificador = notificador;
            _logger = logger;
            _cache = cache;
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

            InvalidarCacheDoUsuario(assinatura.UsuarioId, assinatura.ClienteId);
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

            InvalidarCacheDoUsuario(assinatura.UsuarioId, assinatura.ClienteId);
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
        /// Obtem assinatura ativa com fallback: primeiro por clienteId, depois por userId
        /// </summary>
        public async Task<AssinaturaDTO?> ObterAssinaturaAtivaComFallbackAsync(Guid? clienteId, Guid? userId)
        {
            if (clienteId != null)
            {
                var assinatura = await ObterAssinaturaAtivaDoClienteAsync(clienteId.Value);
                if (assinatura != null) return assinatura;
            }

            if (userId != null)
            {
                return await ObterAssinaturaAtivaDoUsuarioAsync(userId.Value);
            }

            return null;
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
        /// Lista historico de pagamentos via Gateway (fonte da verdade).
        /// Resultado cacheado por 1 hora por usuario.
        /// </summary>
        public async Task<List<HistoricoPagamentoDTO>> ListarHistoricoPagamentosViaGatewayAsync(Guid usuarioId)
        {
            var cacheKey = $"historico_pagamentos_{usuarioId}";
            if (_cache.TryGetValue(cacheKey, out List<HistoricoPagamentoDTO>? cached) && cached != null)
                return cached;

            var pagamentos = await _gatewayService.ListarPagamentosDoUsuarioAsync(usuarioId);

            var resultado = pagamentos.Select(p => new HistoricoPagamentoDTO
            {
                Id = p.Id,
                AssinaturaId = p.AssinaturaId,
                PlanoNome = p.PlanoNome ?? "Plano",
                Valor = p.Valor,
                MetodoPagamento = p.MetodoPagamento,
                Status = p.Status,
                DataPagamento = p.DataPagamento
            }).ToList();

            _cache.Set(cacheKey, resultado, CacheDuration);
            return resultado;
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
        /// Verifica status da licenca com fallback gateway → banco local → beneficio da duvida.
        /// Resultado cacheado por 1 hora por usuario.
        /// </summary>
        public async Task<StatusLicencaResponseDTO> VerificarStatusLicencaAsync(Guid? userId, Guid? clienteId, LicenseService licenseService)
        {
            var cacheKey = $"licenca_status_{userId}_{clienteId}";
            if (_cache.TryGetValue(cacheKey, out StatusLicencaResponseDTO? cached) && cached != null)
            {
                _logger.LogDebug("Cache hit para status licenca: {CacheKey}", cacheKey);
                return cached;
            }

            // Tentar verificar via Gateway de Pagamentos primeiro
            var (gatewayResponse, gatewayDisponivel) = await _gatewayService.VerificarLicencaAsync(userId, clienteId);
            StatusLicencaResponseDTO resultado;

            if (gatewayResponse != null)
            {
                resultado = new StatusLicencaResponseDTO
                {
                    TemLicenca = gatewayResponse.TemLicenca,
                    LicencaAtiva = gatewayResponse.LicencaAtiva,
                    StatusMensagem = gatewayResponse.StatusMensagem,
                    DiasRestantes = gatewayResponse.DiasRestantes,
                    Plano = gatewayResponse.Plano,
                    Fonte = "gateway"
                };
            }
            // Fallback: verificar no banco local
            else if (clienteId != null)
            {
                var statusByCliente = await licenseService.VerificarLicencaDoClienteAsync(clienteId.Value);
                if (statusByCliente.LicencaAtiva || gatewayDisponivel)
                {
                    resultado = new StatusLicencaResponseDTO
                    {
                        TemLicenca = statusByCliente.TemLicenca,
                        LicencaAtiva = statusByCliente.LicencaAtiva,
                        StatusMensagem = statusByCliente.StatusMensagem,
                        DiasRestantes = statusByCliente.DiasRestantes
                    };
                }
                else
                {
                    resultado = await VerificarStatusPorUsuarioAsync(userId, gatewayDisponivel, licenseService);
                }
            }
            else
            {
                resultado = await VerificarStatusPorUsuarioAsync(userId, gatewayDisponivel, licenseService);
            }

            _cache.Set(cacheKey, resultado, CacheDuration);
            return resultado;
        }

        private async Task<StatusLicencaResponseDTO> VerificarStatusPorUsuarioAsync(Guid? userId, bool gatewayDisponivel, LicenseService licenseService)
        {
            if (userId != null)
            {
                var statusByUser = await licenseService.VerificarLicencaDoUsuarioAsync(userId.Value);

                // Se gateway indisponivel e banco local diz sem licenca, dar beneficio da duvida
                if (!gatewayDisponivel && !statusByUser.LicencaAtiva && statusByUser.TemLicenca)
                {
                    statusByUser.LicencaAtiva = true;
                    statusByUser.StatusMensagem = "Licenca ativa (verificacao offline)";
                }

                return new StatusLicencaResponseDTO
                {
                    TemLicenca = statusByUser.TemLicenca,
                    LicencaAtiva = statusByUser.LicencaAtiva,
                    StatusMensagem = statusByUser.StatusMensagem,
                    DiasRestantes = statusByUser.DiasRestantes
                };
            }

            return new StatusLicencaResponseDTO
            {
                TemLicenca = false,
                LicencaAtiva = false,
                StatusMensagem = "Nenhuma licenca encontrada. Adquira um plano para continuar.",
                Alertas = new List<string> { "Sem licenca ativa" }
            };
        }

        /// <summary>
        /// Calcula opcoes de parcelamento para cartao de credito
        /// </summary>
        public ParcelamentoResponseDTO CalcularParcelas(string bandeira, int valorCentavos)
        {
            var valorReais = valorCentavos / 100m;

            var maxParcelas = 12;
            var parcelasSemJuros = 3;
            var taxaJurosMensal = 0.0199m; // 1.99% ao mes

            var parcelas = new List<ParcelaDTO>();

            for (int i = 1; i <= maxParcelas; i++)
            {
                decimal valorParcela;
                decimal valorTotal;
                bool temJuros = i > parcelasSemJuros;
                decimal percentualJuros = 0;

                if (temJuros)
                {
                    var fator = (decimal)Math.Pow((double)(1 + taxaJurosMensal), i);
                    valorTotal = valorReais * fator;
                    valorParcela = Math.Round(valorTotal / i, 2);
                    percentualJuros = Math.Round((valorTotal / valorReais - 1) * 100, 2);
                }
                else
                {
                    valorTotal = valorReais;
                    valorParcela = Math.Round(valorReais / i, 2);
                }

                // Valor minimo por parcela (R$ 5,00)
                if (valorParcela < 5) break;

                parcelas.Add(new ParcelaDTO
                {
                    Parcela = i,
                    Valor = (int)(valorParcela * 100),
                    ValorReais = valorParcela,
                    ValorFormatado = valorParcela.ToString("C", new System.Globalization.CultureInfo("pt-BR")),
                    ValorTotal = (int)(valorTotal * 100),
                    ValorTotalReais = Math.Round(valorTotal, 2),
                    ValorTotalFormatado = Math.Round(valorTotal, 2).ToString("C", new System.Globalization.CultureInfo("pt-BR")),
                    TemJuros = temJuros,
                    PercentualJuros = percentualJuros
                });
            }

            return new ParcelamentoResponseDTO { Parcelas = parcelas, Bandeira = bandeira };
        }

        /// <summary>
        /// Cria assinatura com PIX via Gateway
        /// </summary>
        public async Task<AssinaturaResultDTO<AssinaturaPixResponseDTO>> CriarAssinaturaComPixAsync(Guid planoId, Guid userId, Guid? clienteId)
        {
            var (response, errorMessage) = await _gatewayService.CriarAssinaturaPixAsync(planoId, userId, clienteId);

            if (response == null)
                return AssinaturaResultDTO<AssinaturaPixResponseDTO>.Falha(errorMessage ?? "Erro ao criar assinatura PIX. Tente novamente.");

            InvalidarCacheDoUsuario(userId, clienteId);
            return AssinaturaResultDTO<AssinaturaPixResponseDTO>.Ok(new AssinaturaPixResponseDTO
            {
                AssinaturaId = response.Assinatura?.Id,
                QrCode = response.PixCopiaCola,
                QrCodeImagem = response.PixQrCodeBase64,
                TxId = response.PagamentoId.ToString(),
                DataExpiracao = DateTime.UtcNow.AddHours(24)
            });
        }

        /// <summary>
        /// Cria assinatura com Boleto via Gateway.
        /// Busca nome/CPF do usuario automaticamente se nao fornecidos.
        /// </summary>
        public async Task<AssinaturaResultDTO<AssinaturaBoletoResponseDTO>> CriarAssinaturaComBoletoAsync(
            Guid planoId, Guid userId, string nomePagador, string cpfCnpj, Guid? clienteId)
        {
            // Auto-completar dados do usuario se nao fornecidos
            (nomePagador, cpfCnpj) = await PreencherDadosUsuarioAsync(userId, nomePagador, cpfCnpj);

            if (string.IsNullOrWhiteSpace(nomePagador))
                return AssinaturaResultDTO<AssinaturaBoletoResponseDTO>.Falha("Nome do pagador e obrigatorio para boleto. Atualize seu perfil.");

            if (string.IsNullOrWhiteSpace(cpfCnpj))
                return AssinaturaResultDTO<AssinaturaBoletoResponseDTO>.Falha("CPF/CNPJ e obrigatorio para boleto. Atualize seu perfil.");

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaBoletoAsync(planoId, userId, nomePagador, cpfCnpj, clienteId);

            if (response == null)
                return AssinaturaResultDTO<AssinaturaBoletoResponseDTO>.Falha(errorMessage ?? "Erro ao criar assinatura com boleto. Tente novamente.");

            InvalidarCacheDoUsuario(userId, clienteId);
            return AssinaturaResultDTO<AssinaturaBoletoResponseDTO>.Ok(new AssinaturaBoletoResponseDTO
            {
                AssinaturaId = response.Assinatura?.Id,
                CodigoBarras = response.BoletoCodigoBarras,
                Url = response.BoletoUrl,
                TxId = response.PagamentoId.ToString(),
                DataVencimento = DateTime.UtcNow.AddDays(3)
            });
        }

        /// <summary>
        /// Cria assinatura com Pix Automatico via Gateway.
        /// Busca nome/CPF do usuario automaticamente se nao fornecidos.
        /// </summary>
        public async Task<AssinaturaResultDTO<AssinaturaPixAutomaticoResponseDTO>> CriarAssinaturaComPixAutomaticoAsync(
            Guid planoId, Guid userId, string nomeDevedor, string cpfCnpj, string periodicidade, Guid? clienteId)
        {
            // Auto-completar dados do usuario se nao fornecidos
            (nomeDevedor, cpfCnpj) = await PreencherDadosUsuarioAsync(userId, nomeDevedor, cpfCnpj);

            if (string.IsNullOrWhiteSpace(nomeDevedor))
                return AssinaturaResultDTO<AssinaturaPixAutomaticoResponseDTO>.Falha("Nome do devedor e obrigatorio para Pix Automatico. Atualize seu perfil.");

            if (string.IsNullOrWhiteSpace(cpfCnpj))
                return AssinaturaResultDTO<AssinaturaPixAutomaticoResponseDTO>.Falha("CPF/CNPJ e obrigatorio para Pix Automatico. Atualize seu perfil.");

            if (string.IsNullOrWhiteSpace(periodicidade))
                periodicidade = "ANUAL";

            var (response, errorMessage) = await _gatewayService.CriarAssinaturaPixAutomaticoAsync(
                planoId, userId, nomeDevedor, cpfCnpj, periodicidade, clienteId);

            if (response == null)
                return AssinaturaResultDTO<AssinaturaPixAutomaticoResponseDTO>.Falha(errorMessage ?? "Erro ao criar assinatura com Pix Automatico. Tente novamente.");

            InvalidarCacheDoUsuario(userId, clienteId);
            return AssinaturaResultDTO<AssinaturaPixAutomaticoResponseDTO>.Ok(new AssinaturaPixAutomaticoResponseDTO
            {
                AssinaturaId = response.Assinatura?.Id,
                IdRec = response.IdRec,
                Contrato = response.Contrato,
                StatusRecorrencia = response.StatusRecorrencia,
                QrCode = response.PixCopiaCola,
                QrCodeImagem = response.PixQrCodeBase64,
                LinkAutorizacao = response.LinkAutorizacao,
                TxId = response.PagamentoId.ToString()
            });
        }

        /// <summary>
        /// Cria assinatura com Cartao via Gateway.
        /// Suporta modo payment_token (seguro) e modo legado (dados do cartao).
        /// </summary>
        public async Task<AssinaturaResultDTO<AssinaturaCartaoResponseDTO>> CriarAssinaturaComCartaoAsync(
            Guid planoId, Guid userId, CriarAssinaturaCartaoDTO dto, Guid? clienteId)
        {
            bool usarToken = !string.IsNullOrWhiteSpace(dto.PaymentToken);

            GatewayAssinaturaCartaoResponse? response;
            string? errorMessage;

            if (usarToken)
            {
                // Modo payment_token
                if (string.IsNullOrWhiteSpace(dto.NomePagador))
                    return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha("Nome do pagador e obrigatorio para pagamento com token.");

                if (string.IsNullOrWhiteSpace(dto.CpfCnpj))
                    return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha("CPF/CNPJ e obrigatorio para pagamento com token.");

                (response, errorMessage) = await _gatewayService.CriarAssinaturaCartaoComTokenAsync(
                    planoId, userId, dto.PaymentToken!, dto.NomePagador, dto.CpfCnpj,
                    dto.Email, dto.Telefone, dto.Parcelas, dto.Bandeira, clienteId);
            }
            else
            {
                // Modo legado
                if (string.IsNullOrWhiteSpace(dto.NumeroCartao))
                    return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha("Numero do cartao e obrigatorio.");

                if (string.IsNullOrWhiteSpace(dto.Cvv))
                    return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha("CVV do cartao e obrigatorio.");

                if (string.IsNullOrWhiteSpace(dto.MesValidade) || string.IsNullOrWhiteSpace(dto.AnoValidade))
                    return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha("Data de validade do cartao e obrigatoria.");

                if (string.IsNullOrWhiteSpace(dto.NomeCartao))
                    return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha("Nome no cartao e obrigatorio.");

                (response, errorMessage) = await _gatewayService.CriarAssinaturaCartaoAsync(
                    planoId, userId, dto.NumeroCartao!, dto.Cvv!, dto.MesValidade!,
                    dto.AnoValidade!, dto.NomeCartao!, dto.Parcelas, dto.Bandeira, clienteId);
            }

            if (response == null)
                return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Falha(errorMessage ?? "Erro ao criar assinatura com cartao. Tente novamente.");

            InvalidarCacheDoUsuario(userId, clienteId);
            return AssinaturaResultDTO<AssinaturaCartaoResponseDTO>.Ok(new AssinaturaCartaoResponseDTO
            {
                AssinaturaId = response.Assinatura?.Id,
                Ativa = response.Assinatura?.Ativa ?? false,
                ChargeId = response.ChargeId,
                Status = response.Status,
                Autorizacao = response.Autorizacao,
                Parcelas = response.Parcelas,
                ValorTotal = response.ValorTotal,
                TxId = response.PagamentoId.ToString()
            });
        }

        /// <summary>
        /// Obtem o clienteId a partir do usuarioId
        /// </summary>
        public Guid? ObterClienteIdDoUsuario(Guid userId)
        {
            var clientes = _clienteRepo.ListarTodosClientes(userId);
            var cliente = clientes.FirstOrDefault();
            return cliente?.Id;
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

            InvalidarCacheDoUsuario(assinatura.UsuarioId, assinatura.ClienteId);
            _logger.LogInformation("Assinatura {AssinaturaId} ativada via webhook, pagamento: {PagamentoId}",
                assinaturaId, pagamentoId);

            return true;
        }

        /// <summary>
        /// Invalida cache de assinatura para um usuario/cliente apos mudancas
        /// </summary>
        public void InvalidarCacheDoUsuario(Guid? userId, Guid? clienteId)
        {
            if (userId != null)
            {
                _cache.Remove($"licenca_status_{userId}_{clienteId}");
                _cache.Remove($"licenca_status_{userId}_");
                _cache.Remove($"historico_pagamentos_{userId}");
                _logger.LogDebug("Cache invalidado para usuario {UserId}", userId);
            }
            if (clienteId != null)
            {
                _cache.Remove($"licenca_status__{clienteId}");
            }
        }

        /// <summary>
        /// Busca nome e CPF/CNPJ do usuario se os valores fornecidos estiverem vazios
        /// </summary>
        private async Task<(string nome, string cpfCnpj)> PreencherDadosUsuarioAsync(Guid userId, string nome, string cpfCnpj)
        {
            if (!string.IsNullOrWhiteSpace(nome) && !string.IsNullOrWhiteSpace(cpfCnpj))
                return (nome, cpfCnpj);

            var usuario = await _usuarioRepo.ObterPorIdAsync(userId);
            if (usuario != null)
            {
                if (string.IsNullOrWhiteSpace(nome))
                    nome = usuario.NomeCompleto;
                if (string.IsNullOrWhiteSpace(cpfCnpj))
                    cpfCnpj = usuario.CPF;
            }

            return (nome, cpfCnpj);
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
