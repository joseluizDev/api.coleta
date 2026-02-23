using api.cliente.Interfaces;
using api.coleta.Data;
using api.coleta.Tests.Fakes;
using api.coleta.Tests.Integration.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;

namespace api.coleta.Tests.Integration;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    public Guid TestUserId { get; set; } = Guid.NewGuid();
    public FakeMinioStorage MinioStorage { get; private set; } = new();
    public FakeJwtToken JwtToken { get; private set; } = null!;
    private readonly string _databaseName = $"ApiColetaTests_{Guid.NewGuid()}";

    public TestApplicationFactory()
    {
        // Set dummy environment variables for tests
        Environment.SetEnvironmentVariable("DB_SERVER", "localhost");
        Environment.SetEnvironmentVariable("DB_PORT", "3306");
        Environment.SetEnvironmentVariable("DB_NAME", "test_db");
        Environment.SetEnvironmentVariable("DB_USER", "test_user");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "test_password");
        Environment.SetEnvironmentVariable("APPLY_MIGRATIONS_ON_STARTUP", "false");
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test_jwt_secret_key_12345678901234567890");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test_issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test_audience");
        Environment.SetEnvironmentVariable("MINIO_ENDPOINT", "localhost:9000");
        Environment.SetEnvironmentVariable("MINIO_ACCESS_KEY", "test_access_key");
        Environment.SetEnvironmentVariable("MINIO_SECRET_KEY", "test_secret_key");
        Environment.SetEnvironmentVariable("MINIO_PUBLIC_URL", "http://localhost:9000");
        Environment.SetEnvironmentVariable("GOOGLE_API_KEY", "test_google_api_key");
        Environment.SetEnvironmentVariable("ONESIGNAL_API_KEY", "test_onesignal_api_key");
        Environment.SetEnvironmentVariable("ONESIGNAL_APP_ID", "test_onesignal_app_id");
        Environment.SetEnvironmentVariable("ONESIGNAL_API_URL", "https://api.onesignal.com/notifications");
        Environment.SetEnvironmentVariable("ZEPTOMAIL_API_KEY", "test_zeptomail_api_key");
        Environment.SetEnvironmentVariable("ZEPTOMAIL_API_URL", "https://api.zeptomail.com/v1.1/email");
        Environment.SetEnvironmentVariable("ZEPTOMAIL_FROM_EMAIL", "test@test.com");
        Environment.SetEnvironmentVariable("COMPANY_LOGO_URL", "https://test.com/logo.png");
        Environment.SetEnvironmentVariable("COMPANY_WHATSAPP_LINK", "https://wa.me/5511999999999");
        Environment.SetEnvironmentVariable("COMPANY_WHATSAPP_NUMBER", "5511999999999");
        Environment.SetEnvironmentVariable("ADMIN_EMAILS", "admin@test.com");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(WebHostDefaults.ContentRootKey, Directory.GetCurrentDirectory());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName)
            );

            services.RemoveAll<IMinioStorage>();
            var fakeMinio = new FakeMinioStorage();
            services.AddSingleton<IMinioStorage>(fakeMinio);
            MinioStorage = fakeMinio;

            services.RemoveAll<IJwtToken>();
            var fakeJwt = new FakeJwtToken(() => TestUserId);
            services.AddSingleton<IJwtToken>(fakeJwt);
            JwtToken = fakeJwt;

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });
    }
}
