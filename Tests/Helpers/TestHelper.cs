using AutoMapper;
using Microsoft.EntityFrameworkCore;

public static class TestHelper
{
    public static ApplicationDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        return configuration.CreateMapper();
    }
}
