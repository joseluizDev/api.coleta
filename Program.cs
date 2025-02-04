
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
using api.funcionario.Services;
using api.funcionario.Repositories;
using Microsoft.OpenApi.Models;
using System.Text;
using api.cliente.Interfaces;
using BackAppPromo.Infrastructure.Authentication;
using api.minionStorage.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSwaggerGen(c =>
{
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


builder.Services.AddSingleton<IMinioStorage>(provider =>
    new MinioStorage(
        endpoint: builder.Configuration["Minio:Endpoint"],
        accessKey: builder.Configuration["Minio:AccessKey"],
        secretKey: builder.Configuration["Minio:SecretKey"]
    )
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUnitOfWork, UnitOfWorkImplements>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ApplicationDbContext>();


builder.Services.AddOutputCache();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var versao = ServerVersion.AutoDetect(connectionString);

    options.UseMySql(connectionString, versao);
});



builder.Services.Configure<GoogleApiSettings>(builder.Configuration.GetSection("GoogleApi"));


builder.Services.AddScoped<IJwtToken, JwtTokenService>();

builder.Services.AddScoped<INotificador, Notificador>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<ColetaRepository>();
builder.Services.AddScoped<ColetaService>();

builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<ClienteService>();

builder.Services.AddScoped<FuncionarioRepository>();
builder.Services.AddScoped<FuncionarioService>();

builder.Services.AddScoped<FazendaRepository>();
builder.Services.AddScoped<FazendaService>();

builder.Services.AddScoped<TalhaoRepository>();
builder.Services.AddScoped<TalhaoService>();

builder.Services.AddScoped<VinculoClienteFazendaRepository>();
builder.Services.AddScoped<VinculoClienteFazendaService>();

builder.Services.AddScoped<SafraRepository>();
builder.Services.AddScoped<SafraService>();

builder.Services.AddScoped<UtilsService>();




builder.Services.AddAutoMapper(typeof(MappingProfile));

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        MyAllowSpecificOrigins,
        builder => builder.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

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

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            ),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddMvc().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseOutputCache();

app.Run();
