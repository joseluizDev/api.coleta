
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseOutputCache();

app.Run();

public partial class Program
{
}
