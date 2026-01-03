
using api.coleta.repositories;
using api.coleta.Repositories;
using api.coleta.Settings;
using Microsoft.EntityFrameworkCore;
using api.coleta.Data;
using api.coleta.Data.Repository;
using api.cliente.Repositories;
using api.cliente.Services;
using api.fazenda.repositories;
using api.talhao.Repositories;
using api.talhao.Services;
using api.vinculoClienteFazenda.Services;
using api.safra.Repositories;
using api.safra.Services;
using Microsoft.OpenApi.Models;
using System.Text;
using api.cliente.Interfaces;
using BackAppPromo.Infrastructure.Authentication;
using api.minionStorage.Services;
using api.coleta.Services;
using api.dashboard.Services;
using DotNetEnv;
using api.coleta.Jobs;
using api.coleta.Interfaces;
using api.coleta.Middleware;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Inject environment variables into configuration for services that use IConfiguration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Jwt:SecretKey"] = Environment.GetEnvironmentVariable("JWT_SECRET_KEY"),
    ["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    ["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),

});



builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Coleta",
        Version = "v1",
        Description = "API para gerenciamento de coletas agrícolas"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu-token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUnitOfWork, UnitOfWorkImplements>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ApplicationDbContext>();


builder.Services.AddOutputCache();


// Build connection string from environment variables only
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER")
    ?? throw new InvalidOperationException("DB_SERVER não configurado no .env");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT")
    ?? throw new InvalidOperationException("DB_PORT não configurado no .env");
var dbName = Environment.GetEnvironmentVariable("DB_NAME")
    ?? throw new InvalidOperationException("DB_NAME não configurado no .env");
var dbUser = Environment.GetEnvironmentVariable("DB_USER")
    ?? throw new InvalidOperationException("DB_USER não configurado no .env");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
    ?? throw new InvalidOperationException("DB_PASSWORD não configurado no .env");

var connectionString = $"server={dbServer};port={dbPort};database={dbName};user={dbUser};password={dbPassword};";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var versao = ServerVersion.AutoDetect(connectionString);
    options.UseMySql(connectionString, versao);
});



builder.Services.Configure<GoogleApiSettings>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
        ?? throw new InvalidOperationException("GOOGLE_API_KEY não configurado no .env");
});


builder.Services.AddScoped<IJwtToken, JwtTokenService>();

builder.Services.AddScoped<INotificador, Notificador>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<ColetaRepository>();
builder.Services.AddScoped<ColetaService>();

builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<ClienteService>();

builder.Services.AddScoped<ConfiguracaoPadraoRepository>();
builder.Services.AddScoped<ConfiguracaoPadraoService>();

builder.Services.AddScoped<ConfiguracaoPersonalizadaRepository>();
builder.Services.AddScoped<ConfiguracaoPersonalizadaService>();

builder.Services.AddScoped<FazendaRepository>();
builder.Services.AddScoped<FazendaService>();

builder.Services.AddScoped<MineralRepository>();
builder.Services.AddScoped<MineralService>();

builder.Services.AddScoped<TalhaoRepository>();
builder.Services.AddScoped<TalhaoService>();

builder.Services.AddScoped<VinculoClienteFazendaRepository>();
builder.Services.AddScoped<VinculoClienteFazendaService>();

builder.Services.AddScoped<SafraRepository>();
builder.Services.AddScoped<SafraService>();

builder.Services.AddScoped<UtilsService>();

builder.Services.AddScoped<VisualizarMapaRepository>();
builder.Services.AddScoped<PontoColetadoRepository>();
builder.Services.AddScoped<api.coleta.Interfaces.IOneSignalService, api.coleta.Services.OneSignalService>();
builder.Services.AddScoped<VisualizarMapaService>(provider =>
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

builder.Services.AddScoped<GeoJsonRepository>();
builder.Services.AddScoped<GeoJsonService>();

builder.Services.AddScoped<RelatorioRepository>();

// Services extraídos do RelatorioService para melhor separação de responsabilidades
builder.Services.AddScoped<api.coleta.Services.Relatorio.NutrientClassificationService>();
builder.Services.AddScoped<api.coleta.Services.Relatorio.GeoJsonProcessorService>();
builder.Services.AddScoped<api.coleta.Services.Relatorio.AttributeStatisticsService>();
builder.Services.AddScoped<api.coleta.Services.Relatorio.SoilIndicatorService>();

builder.Services.AddScoped<RelatorioService>();

builder.Services.AddScoped<RecomendacaoRepository>();
builder.Services.AddScoped<RecomendacaoService>();

builder.Services.AddScoped<ImagemNdviRepository>();
builder.Services.AddScoped<ImagemNdviService>();

builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<NutrientConfigRepository>();
builder.Services.AddScoped<NutrientConfigService>();

builder.Services.AddScoped<MensagemAgendadaRepository>();
builder.Services.AddScoped<MensagemAgendadaService>();

builder.Services.AddScoped<ContatoRepository>();
builder.Services.AddScoped<api.coleta.Interfaces.IZeptomailService, ZeptomailService>();
builder.Services.AddScoped<ContatoService>();

builder.Services.AddHostedService<MensagemAgendadaJob>();

// ===== LICENSING SYSTEM =====
// EfiPay Configuration - Credenciais de Produção
var basePath = AppContext.BaseDirectory;
var certFileName = "producao-643354-AgroSyste.p12";
var certPath = Path.Combine(basePath, certFileName);

// Fallback para desenvolvimento local
if (!File.Exists(certPath))
{
    certPath = Path.Combine(Directory.GetCurrentDirectory(), certFileName);
}

builder.Services.Configure<EfiPaySettings>(options =>
{
    // Credenciais EfiPay (Produção)
    options.ClientId = "Client_Id_17711359c8b4a9ce370814111e98a3e1c4821443";
    options.ClientSecret = "Client_Secret_a1e623c3bd3f90262b377c9ab167def9b9d89234";
    options.ChavePix = "43f89047-906c-4876-b9d5-1c3149cbff95";
    options.CertificadoPath = certPath;

    // Webhook URLs para EfiPay (skip-mTLS com validação de IP + HMAC)
    // PIX: /api/webhook/efipay (Efi Pay adiciona /pix automaticamente)
    // Cobranças/Assinaturas Recorrentes: /api/webhook/efipay/cobranca
    // IMPORTANTE: hmac deve ser igual ao WEBHOOK_HMAC_SECRET no WebhookController
    options.WebhookUrl = "https://apis-api-coleta.w4dxlp.easypanel.host/api/webhook/efipay?hmac=agrosyste_webhook_2024_secret&ignorar=";
    options.UseSandbox = false; // Produção
});

// Licensing Repositories
builder.Services.AddScoped<PlanoRepository>();
builder.Services.AddScoped<AssinaturaRepository>();
builder.Services.AddScoped<HistoricoPagamentoRepository>();

// Licensing Services
builder.Services.AddScoped<IEfiPayService, EfiPayService>();
builder.Services.AddScoped<IEfiPayBoletoService, EfiPayBoletoService>();
builder.Services.AddScoped<IEfiPayCartaoService, EfiPayCartaoService>();
builder.Services.AddScoped<IEfiPayAssinaturaService, EfiPayAssinaturaService>();
builder.Services.AddScoped<PlanoService>();
builder.Services.AddScoped<AssinaturaService>();
builder.Services.AddScoped<LicenseService>();

string corsPolicyName = "AllowAnyOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure MinIO from environment variables only
var minioEndpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT")
    ?? throw new InvalidOperationException("MINIO_ENDPOINT não configurado no .env");

var minioAccessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY")
    ?? throw new InvalidOperationException("MINIO_ACCESS_KEY não configurado no .env");

var minioSecretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")
    ?? throw new InvalidOperationException("MINIO_SECRET_KEY não configurado no .env");

var minioPublicUrl = Environment.GetEnvironmentVariable("MINIO_PUBLIC_URL")
    ?? throw new InvalidOperationException("MINIO_PUBLIC_URL não configurado no .env");

builder.Services.AddSingleton<IMinioStorage>(provider =>
    new MinioStorage(
        endpoint: minioEndpoint,
        accessKey: minioAccessKey,
        secretKey: minioSecretKey,
        publicUrl: minioPublicUrl
    )
);

// Configure JWT from environment variables only
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? throw new InvalidOperationException("JWT_SECRET_KEY não configurado no .env");

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new InvalidOperationException("JWT_ISSUER não configurado no .env");

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado no .env");

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)
            ),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddMvc().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Aplicar migrations automaticamente se configurado
var applyMigrations = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS_ON_STARTUP")?.ToLower() == "true";

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();

        if (pendingMigrations.Any())
        {
            logger.LogInformation("Aplicando {Count} migration(s) pendente(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            db.Database.Migrate();

            logger.LogInformation("Migrations aplicadas com sucesso!");
        }
        else
        {
            logger.LogInformation("Banco de dados já está atualizado. Nenhuma migration pendente.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao aplicar migrations. A aplicação continuará, mas o banco pode estar desatualizado.");
        // Em produção, você pode querer lançar a exceção para impedir o startup
        // throw;
    }
}

app.UseCors(corsPolicyName);

// Fix licensing tables if needed (recreate with correct schema)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Check if licensing tables need to be recreated (check for missing columns)
        var needsRecreate = false;
        try
        {
            // Check Planos table
            db.Database.ExecuteSqlRaw("SELECT ValorAnual FROM Planos LIMIT 1");
            // Check Assinaturas table has all required columns
            db.Database.ExecuteSqlRaw("SELECT Ativa, AutoRenovar, StatusPagamento FROM Assinaturas LIMIT 1");
        }
        catch
        {
            needsRecreate = true;
        }

        if (needsRecreate)
        {
            Console.WriteLine("Recreating licensing tables with correct schema...");

            // Drop existing tables with FK checks disabled in same statement
            db.Database.ExecuteSqlRaw(@"
                SET FOREIGN_KEY_CHECKS = 0;
                DROP TABLE IF EXISTS `HistoricosPagamento`;
                DROP TABLE IF EXISTS `Assinaturas`;
                DROP TABLE IF EXISTS `Planos`;
                SET FOREIGN_KEY_CHECKS = 1;
            ");

            // Recreate Planos table
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE `Planos` (
                    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
                    `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `ValorAnual` decimal(12,4) NOT NULL,
                    `LimiteHectares` decimal(12,4) NOT NULL,
                    `Ativo` tinyint(1) NOT NULL DEFAULT 1,
                    `RequereContato` tinyint(1) NOT NULL DEFAULT 0,
                    `EfiPayPlanId` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `DataInclusao` datetime(6) NOT NULL,
                    CONSTRAINT `PK_Planos` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4
            ");

            // Recreate Assinaturas table with all columns
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE `Assinaturas` (
                    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
                    `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
                    `PlanoId` char(36) COLLATE ascii_general_ci NOT NULL,
                    `DataInicio` datetime(6) NOT NULL,
                    `DataFim` datetime(6) NOT NULL,
                    `Ativa` tinyint(1) NOT NULL DEFAULT 0,
                    `AutoRenovar` tinyint(1) NOT NULL DEFAULT 0,
                    `Observacao` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `DeletadoEm` datetime(6) NULL,
                    `EfiPaySubscriptionId` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `EfiPayPlanId` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `StatusPagamento` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `DataUltimoPagamento` datetime(6) NULL,
                    `DataInclusao` datetime(6) NOT NULL,
                    CONSTRAINT `PK_Assinaturas` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_Assinaturas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
                    CONSTRAINT `FK_Assinaturas_Planos_PlanoId` FOREIGN KEY (`PlanoId`) REFERENCES `Planos` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4
            ");

            // Recreate HistoricosPagamento table with all columns
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE `HistoricosPagamento` (
                    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
                    `AssinaturaId` char(36) COLLATE ascii_general_ci NOT NULL,
                    `Valor` decimal(12,4) NOT NULL,
                    `DataPagamento` datetime(6) NOT NULL,
                    `MetodoPagamento` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    `Status` int NOT NULL DEFAULT 0,
                    `TransacaoId` varchar(255) CHARACTER SET utf8mb4 NULL,
                    `Observacao` longtext CHARACTER SET utf8mb4 NULL,
                    `DeletadoEm` datetime(6) NULL,
                    `EfiPayChargeId` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `EfiPayStatus` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `PixQrCode` longtext CHARACTER SET utf8mb4 NULL,
                    `PixQrCodeBase64` longtext CHARACTER SET utf8mb4 NULL,
                    `PixTxId` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `DataExpiracao` datetime(6) NULL,
                    `DataInclusao` datetime(6) NOT NULL,
                    CONSTRAINT `PK_HistoricosPagamento` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_HistoricosPagamento_Assinaturas_AssinaturaId` FOREIGN KEY (`AssinaturaId`) REFERENCES `Assinaturas` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4
            ");

            // Insert default plans
            db.Database.ExecuteSqlRaw(@"
                INSERT INTO `Planos` (`Id`, `Nome`, `Descricao`, `ValorAnual`, `LimiteHectares`, `Ativo`, `RequereContato`, `DataInclusao`)
                VALUES
                (UUID(), 'Básico', 'Plano ideal para pequenos produtores. Inclui todas as funcionalidades essenciais para até 1.000 hectares.', 3598.00, 1000, 1, 0, NOW()),
                (UUID(), 'Premium', 'Plano completo para produtores de médio porte. Todas as funcionalidades para até 2.000 hectares.', 6599.80, 2000, 1, 0, NOW()),
                (UUID(), 'Gold', 'Plano personalizado para grandes produtores. Hectares e funcionalidades sob medida para sua operação.', 0, 999999, 1, 1, NOW())
            ");

            Console.WriteLine("Licensing tables recreated successfully with default plans!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Note: Could not setup licensing tables: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// License validation middleware (after auth, before controllers)
// Uncomment to enable license validation:
// app.UseLicenseValidation();

app.MapControllers();

app.UseOutputCache();

app.Run();

public partial class Program
{
}
