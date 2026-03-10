using api.coleta.Configuration;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Injetar variaveis JWT na configuracao
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Jwt:SecretKey"] = Environment.GetEnvironmentVariable("JWT_SECRET_KEY"),
    ["Jwt:RefreshSecretKey"] = Environment.GetEnvironmentVariable("JWT_REFRESH_SECRET_KEY"),
    ["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    ["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
});

// Configuracoes
builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddDatabaseConfiguration();
builder.Services.AddAuthConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddMinioConfiguration();
builder.Services.AddDependencyInjection();

builder.Services.AddMvc().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Startup do banco de dados
app.ApplyMigrations();
app.EnsureLicensingTables();

// Pipeline HTTP
app.UseCors("AllowAnyOrigin");

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"errors\":[\"Erro interno do servidor\"]}");
    });
});

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();

app.Run();

public partial class Program { }
