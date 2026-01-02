using api.cliente.Repositories;
using api.coleta.Models.DTOs.Licenciamento;
using api.coleta.Repositories;
using api.talhao.Repositories;

namespace api.coleta.Services
{
    public class LicenseService : ServiceBase
    {
        private readonly AssinaturaRepository _assinaturaRepo;
        private readonly ClienteRepository _clienteRepo;
        private readonly TalhaoRepository _talhaoRepo;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(
            AssinaturaRepository assinaturaRepo,
            ClienteRepository clienteRepo,
            TalhaoRepository talhaoRepo,
            ILogger<LicenseService> logger,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _assinaturaRepo = assinaturaRepo;
            _clienteRepo = clienteRepo;
            _talhaoRepo = talhaoRepo;
            _logger = logger;
        }

        public async Task<LicenseStatusDTO> VerificarLicencaDoClienteAsync(Guid clienteId)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);

            if (assinatura == null)
            {
                return new LicenseStatusDTO
                {
                    TemLicenca = false,
                    LicencaAtiva = false,
                    StatusMensagem = "Nenhuma licença ativa encontrada. Adquira um plano para continuar.",
                    Alertas = new List<string> { "Sem licença ativa" }
                };
            }

            var hectaresUtilizados = await CalcularHectaresUtilizadosAsync(clienteId);
            var hectaresDisponiveis = assinatura.Plano?.LimiteHectares ?? 0;
            var percentualUtilizado = hectaresDisponiveis > 0
                ? (hectaresUtilizados / hectaresDisponiveis) * 100
                : 0;

            var diasRestantes = assinatura.DiasRestantes();
            var proximoDoVencimento = diasRestantes <= 30;

            var status = new LicenseStatusDTO
            {
                TemLicenca = true,
                LicencaAtiva = assinatura.EstaVigente(),
                StatusMensagem = assinatura.EstaVigente()
                    ? $"Licença ativa - {assinatura.Plano?.Nome}"
                    : "Licença expirada",
                PlanoAtual = assinatura.Plano != null ? new PlanoDTO
                {
                    Id = assinatura.Plano.Id,
                    Nome = assinatura.Plano.Nome,
                    Descricao = assinatura.Plano.Descricao,
                    ValorAnual = assinatura.Plano.ValorAnual,
                    LimiteHectares = assinatura.Plano.LimiteHectares,
                    Ativo = assinatura.Plano.Ativo,
                    RequereContato = assinatura.Plano.RequereContato
                } : null,
                AssinaturaAtual = new AssinaturaDTO
                {
                    Id = assinatura.Id,
                    ClienteId = assinatura.ClienteId,
                    PlanoId = assinatura.PlanoId,
                    DataInicio = assinatura.DataInicio,
                    DataFim = assinatura.DataFim,
                    Ativa = assinatura.Ativa,
                    EstaVigente = assinatura.EstaVigente(),
                    DiasRestantes = diasRestantes,
                    StatusPagamento = assinatura.StatusPagamento
                },
                HectaresUtilizados = hectaresUtilizados,
                HectaresDisponiveis = hectaresDisponiveis - hectaresUtilizados,
                PercentualUtilizado = Math.Round(percentualUtilizado, 1),
                DiasRestantes = diasRestantes,
                ProximoDoVencimento = proximoDoVencimento,
                DataVencimento = assinatura.DataFim,
                Alertas = new List<string>()
            };

            // Add alerts
            if (proximoDoVencimento && diasRestantes > 0)
            {
                status.Alertas.Add($"Sua licença expira em {diasRestantes} dias. Renove agora!");
            }

            if (percentualUtilizado >= 90)
            {
                status.Alertas.Add($"Você está utilizando {percentualUtilizado:F1}% do limite de hectares.");
            }

            if (!assinatura.EstaVigente())
            {
                status.Alertas.Add("Sua licença expirou. Renove para continuar usando o sistema.");
            }

            return status;
        }

        public async Task<ValidacaoLicencaResult> ValidarLicencaAsync(Guid clienteId)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);

            if (assinatura == null)
            {
                return new ValidacaoLicencaResult
                {
                    Valida = false,
                    Motivo = "Nenhuma licença ativa encontrada.",
                    ClienteId = clienteId
                };
            }

            if (!assinatura.EstaVigente())
            {
                return new ValidacaoLicencaResult
                {
                    Valida = false,
                    Motivo = "Licença expirada.",
                    ClienteId = clienteId,
                    AssinaturaId = assinatura.Id,
                    DiasRestantes = 0
                };
            }

            return new ValidacaoLicencaResult
            {
                Valida = true,
                Motivo = "Licença válida",
                ClienteId = clienteId,
                AssinaturaId = assinatura.Id,
                DiasRestantes = assinatura.DiasRestantes()
            };
        }

        public async Task<bool> ValidarLimiteHectaresAsync(Guid clienteId, decimal hectaresAdicionais)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(clienteId);

            if (assinatura == null || !assinatura.EstaVigente())
            {
                return false;
            }

            var hectaresAtuais = await CalcularHectaresUtilizadosAsync(clienteId);
            var limiteHectares = assinatura.Plano?.LimiteHectares ?? 0;

            return (hectaresAtuais + hectaresAdicionais) <= limiteHectares;
        }

        private async Task<decimal> CalcularHectaresUtilizadosAsync(Guid clienteId)
        {
            try
            {
                // Get all talhões from fazendas linked to this cliente
                var totalHectares = await _talhaoRepo.ObterTotalHectaresPorClienteAsync(clienteId);
                return totalHectares;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular hectares utilizados para cliente {ClienteId}", clienteId);
                return 0;
            }
        }

        public async Task DesativarAssinaturasExpiradasAsync()
        {
            var expiradas = await _assinaturaRepo.ObterAssinaturasExpiradasAsync();

            foreach (var assinatura in expiradas)
            {
                assinatura.Ativa = false;
                assinatura.StatusPagamento = "expired";
                _assinaturaRepo.Atualizar(assinatura);

                _logger.LogInformation("Assinatura {AssinaturaId} expirada e desativada", assinatura.Id);
            }

            if (expiradas.Any())
            {
                UnitOfWork.Commit();
            }
        }

        /// <summary>
        /// Verifica licenca diretamente pelo UsuarioId (fallback quando clienteId nao encontrado)
        /// </summary>
        public async Task<LicenseStatusDTO> VerificarLicencaDoUsuarioAsync(Guid usuarioId)
        {
            var assinatura = await _assinaturaRepo.ObterAssinaturaAtivaDoUsuarioAsync(usuarioId);

            if (assinatura == null)
            {
                return new LicenseStatusDTO
                {
                    TemLicenca = false,
                    LicencaAtiva = false,
                    StatusMensagem = "Nenhuma licenca ativa encontrada. Adquira um plano para continuar.",
                    Alertas = new List<string> { "Sem licenca ativa" }
                };
            }

            var clienteId = assinatura.ClienteId;
            var hectaresUtilizados = await CalcularHectaresUtilizadosAsync(clienteId);
            var hectaresDisponiveis = assinatura.Plano?.LimiteHectares ?? 0;
            var percentualUtilizado = hectaresDisponiveis > 0
                ? (hectaresUtilizados / hectaresDisponiveis) * 100
                : 0;

            var diasRestantes = assinatura.DiasRestantes();
            var proximoDoVencimento = diasRestantes <= 30;

            var status = new LicenseStatusDTO
            {
                TemLicenca = true,
                LicencaAtiva = assinatura.EstaVigente(),
                StatusMensagem = assinatura.EstaVigente()
                    ? $"Licenca ativa - {assinatura.Plano?.Nome}"
                    : "Licenca expirada",
                PlanoAtual = assinatura.Plano != null ? new PlanoDTO
                {
                    Id = assinatura.Plano.Id,
                    Nome = assinatura.Plano.Nome,
                    Descricao = assinatura.Plano.Descricao,
                    ValorAnual = assinatura.Plano.ValorAnual,
                    LimiteHectares = assinatura.Plano.LimiteHectares,
                    Ativo = assinatura.Plano.Ativo,
                    RequereContato = assinatura.Plano.RequereContato
                } : null,
                AssinaturaAtual = new AssinaturaDTO
                {
                    Id = assinatura.Id,
                    ClienteId = assinatura.ClienteId,
                    PlanoId = assinatura.PlanoId,
                    DataInicio = assinatura.DataInicio,
                    DataFim = assinatura.DataFim,
                    Ativa = assinatura.Ativa,
                    EstaVigente = assinatura.EstaVigente(),
                    DiasRestantes = diasRestantes,
                    StatusPagamento = assinatura.StatusPagamento
                },
                HectaresUtilizados = hectaresUtilizados,
                HectaresDisponiveis = hectaresDisponiveis - hectaresUtilizados,
                PercentualUtilizado = Math.Round(percentualUtilizado, 1),
                DiasRestantes = diasRestantes,
                ProximoDoVencimento = proximoDoVencimento,
                DataVencimento = assinatura.DataFim,
                Alertas = new List<string>()
            };

            // Add alerts
            if (proximoDoVencimento && diasRestantes > 0)
            {
                status.Alertas.Add($"Sua licenca expira em {diasRestantes} dias. Renove agora!");
            }

            if (percentualUtilizado >= 90)
            {
                status.Alertas.Add($"Voce esta utilizando {percentualUtilizado:F1}% do limite de hectares.");
            }

            if (!assinatura.EstaVigente())
            {
                status.Alertas.Add("Sua licenca expirou. Renove para continuar usando o sistema.");
            }

            return status;
        }
    }
}
