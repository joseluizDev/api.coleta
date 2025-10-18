using api.coleta.Services;

namespace api.coleta.Jobs
{
    public class MensagemAgendadaJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MensagemAgendadaJob> _logger;
        private readonly TimeSpan _intervaloVerificacao = TimeSpan.FromMinutes(1); // Verifica a cada 1 minuto

        public MensagemAgendadaJob(
            IServiceProvider serviceProvider,
            ILogger<MensagemAgendadaJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MensagemAgendadaJob iniciado em: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessarMensagensAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagens agendadas");
                }

                await Task.Delay(_intervaloVerificacao, stoppingToken);
            }

            _logger.LogInformation("MensagemAgendadaJob finalizado em: {time}", DateTimeOffset.Now);
        }

        private async Task ProcessarMensagensAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var mensagemService = scope.ServiceProvider.GetRequiredService<MensagemAgendadaService>();

            try
            {
                // var totalPendentes = await mensagemService.ContarMensagensPendentesAsync();

                // if (totalPendentes > 0)
                // {
                //     _logger.LogInformation("Processando {count} mensagens pendentes", totalPendentes);
                //     await mensagemService.ProcessarMensagensPendentesAsync();
                //     _logger.LogInformation("Mensagens processadas com sucesso");
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagens: {message}", ex.Message);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MensagemAgendadaJob está parando");
            return base.StopAsync(cancellationToken);
        }
    }
}
