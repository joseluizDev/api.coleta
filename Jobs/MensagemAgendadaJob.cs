using api.coleta.Services;

namespace api.coleta.Jobs
{
    public class MensagemAgendadaJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MensagemAgendadaJob> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(1);

        public MensagemAgendadaJob(
            IServiceProvider serviceProvider,
            ILogger<MensagemAgendadaJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MensagemAgendadaJob iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Processando mensagens agendadas pendentes...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mensagemService = scope.ServiceProvider.GetRequiredService<MensagemAgendadaService>();
                        await mensagemService.ProcessarMensagensPendentesAsync();
                    }

                    _logger.LogInformation("Processamento de mensagens agendadas concluído.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagens agendadas: {Message}", ex.Message);
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("MensagemAgendadaJob encerrado.");
        }
    }
}
