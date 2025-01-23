
using api.coleta.repositories;
using api.coleta.Repositories; // Possível duplicação ou erro de digitação.
using api.coleta.Settings;
using Microsoft.EntityFrameworkCore;
using api.coleta.Data;
using api.coleta.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<INotificador, Notificador>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddScoped<ColetaRepository>();
builder.Services.AddScoped<ColetaService>();

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

builder.Services.AddMvc().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseOutputCache();

app.Run();
