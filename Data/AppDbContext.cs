using api.coleta.Models.Entidades;
using api.fazenda.Models.Entidades;
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

        modelBuilder.Entity<Cliente>()
        .Property(c => c.Documento)
        .HasMaxLength(14);

   
    }

    public DbSet<MColeta> MColetas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Fazenda> Fazendas { get; set; }
    public DbSet<Talhao> Talhoes { get; set; }
    public DbSet<VinculoClienteFazenda> VinculoClienteFazendas { get; set; }
    public DbSet<ConfiguracaoPersonalizada> ConfiguracaoPersonalizadas { get; set; }
    public DbSet<ConfiguracaoPadrao> ConfiguracaoPadraos { get; set; }
    public DbSet<Safra> Safras { get; set; }
    public DbSet<TalhaoJson> TalhaoJson { get; set; }
    public DbSet<Coleta> Coletas { get; set; }
    public DbSet<Geojson> Geojson { get; set; }
    public DbSet<Minerais> Minerais { get; set; }
    public DbSet<Relatorio> Relatorios { get; set; }
    public DbSet<PontoColetado> PontoColetados { get; set; }
    public DbSet<ImagemNdvi> ImagensNdvi { get; set; }
    public DbSet<NutrientConfig> NutrientConfigs { get; set; }
    public DbSet<MensagemAgendada> MensagensAgendadas { get; set; }
}
