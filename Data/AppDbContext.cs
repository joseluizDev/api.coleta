using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
            .ToList()
            .ForEach(p => p.SetColumnType("decimal(12,4)"));

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Coleta> Coletas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

}
