using api.cliente.Repositories;
using api.cliente.Services;
using api.coleta.Data;
using api.coleta.Data.Repository;
using api.coleta.Jobs;
using api.coleta.repositories;
using api.coleta.Repositories;
using api.coleta.Services;
using api.coleta.Settings;
using api.dashboard.Services;
using api.fazenda.repositories;
using api.safra.Repositories;
using api.safra.Services;
using api.talhao.Repositories;
using api.talhao.Services;
using api.vinculoClienteFazenda.Services;

namespace api.coleta.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            // Infraestrutura
            services.AddScoped<IUnitOfWork, UnitOfWorkImplements>();
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddOutputCache();

            services.AddScoped<INotificador, Notificador>();

            // Usuario
            services.AddScoped<UsuarioRepository>();
            services.AddScoped<UsuarioService>();

            // Coleta
            services.AddScoped<ColetaRepository>();
            services.AddScoped<ColetaService>();

            // Cliente
            services.AddScoped<ClienteRepository>();
            services.AddScoped<ClienteService>();

            // Configuracao
            services.AddScoped<ConfiguracaoPadraoRepository>();
            services.AddScoped<ConfiguracaoPadraoService>();
            services.AddScoped<ConfiguracaoPersonalizadaRepository>();
            services.AddScoped<ConfiguracaoPersonalizadaService>();

            // Fazenda
            services.AddScoped<FazendaRepository>();
            services.AddScoped<FazendaService>();

            // Mineral
            services.AddScoped<MineralRepository>();
            services.AddScoped<MineralService>();

            // Talhao
            services.AddScoped<TalhaoRepository>();
            services.AddScoped<TalhaoService>();

            // Vinculo Cliente-Fazenda
            services.AddScoped<VinculoClienteFazendaRepository>();
            services.AddScoped<VinculoClienteFazendaService>();

            // Safra
            services.AddScoped<SafraRepository>();
            services.AddScoped<SafraService>();

            // Utils
            services.AddScoped<UtilsService>();

            // Visualizar Mapa
            services.AddScoped<VisualizarMapaRepository>();
            services.AddScoped<PontoColetadoRepository>();
            services.AddScoped<api.coleta.Interfaces.IOneSignalService, api.coleta.Services.OneSignalService>();
            services.AddScoped<VisualizarMapaService>(provider =>
                new VisualizarMapaService(
                    provider.GetRequiredService<UsuarioService>(),
                    provider.GetRequiredService<VisualizarMapaRepository>(),
                    provider.GetRequiredService<IUnitOfWork>(),
                    provider.GetRequiredService<GeoJsonRepository>(),
                    provider.GetRequiredService<TalhaoService>(),
                    provider.GetRequiredService<SafraService>(),
                    provider.GetRequiredService<PontoColetadoRepository>(),
                    provider.GetRequiredService<api.coleta.Interfaces.IOneSignalService>(),
                    provider.GetRequiredService<UsuarioRepository>()
                )
            );

            // GeoJson
            services.AddScoped<GeoJsonRepository>();
            services.AddScoped<GeoJsonService>();

            // Relatorio
            services.AddScoped<RelatorioRepository>();
            services.AddScoped<api.coleta.Services.Relatorio.NutrientClassificationService>();
            services.AddScoped<api.coleta.Services.Relatorio.GeoJsonProcessorService>();
            services.AddScoped<api.coleta.Services.Relatorio.AttributeStatisticsService>();
            services.AddScoped<api.coleta.Services.Relatorio.SoilIndicatorService>();
            services.AddScoped<RelatorioService>();

            // Recomendacao
            services.AddScoped<RecomendacaoRepository>();
            services.AddScoped<RecomendacaoService>();

            // NDVI
            services.AddScoped<ImagemNdviRepository>();
            services.AddScoped<ImagemNdviService>();

            // Dashboard
            services.AddScoped<DashboardService>();

            // Nutrient Config
            services.AddScoped<NutrientConfigRepository>();
            services.AddScoped<NutrientConfigService>();

            // Mensagem Agendada
            services.AddScoped<MensagemAgendadaRepository>();
            services.AddScoped<MensagemAgendadaService>();

            // Contato
            services.AddScoped<ContatoRepository>();
            services.AddScoped<api.coleta.Interfaces.IZeptomailService, ZeptomailService>();
            services.AddScoped<ContatoService>();

            // Jobs
            services.AddHostedService<MensagemAgendadaJob>();

            // Licenciamento / Gateway
            services.Configure<GatewaySettings>(options =>
            {
                options.BaseUrl = Environment.GetEnvironmentVariable("GATEWAY_URL") ?? "http://localhost:8001";
                options.ApiKey = Environment.GetEnvironmentVariable("GATEWAY_API_KEY") ?? "agrosyste_gateway_api_key_2024";
                options.TimeoutSeconds = 30;
            });
            services.AddHttpClient<IGatewayService, GatewayService>();
            services.AddScoped<AssinaturaRepository>();
            services.AddScoped<HistoricoPagamentoRepository>();
            services.AddScoped<AssinaturaService>();
            services.AddScoped<LicenseService>();

            // Settings
            services.Configure<GoogleApiSettings>(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
                    ?? throw new InvalidOperationException("GOOGLE_API_KEY não configurado no .env");
            });
            services.Configure<OpenWeatherMapSettings>(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY") ?? string.Empty;
            });

            return services;
        }
    }
}
