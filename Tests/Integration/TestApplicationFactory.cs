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
