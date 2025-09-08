using Bankmore.Accounts.Query.Infrastructure.Db.Configurations;
using Bankmore.Accounts.Query.Infrastructure.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bankmore.Accounts.Query.Infrastructure.Db;

public sealed class AccountsQueryDbContext : DbContext
{
    public AccountsQueryDbContext(DbContextOptions<AccountsQueryDbContext> options) : base(options) { }

    public DbSet<ContaCorrente> Contas => Set<ContaCorrente>();
    public DbSet<Movimento> Movimentos => Set<Movimento>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContaCorrenteConfig());
        modelBuilder.ApplyConfiguration(new MovimentoConfig());
        modelBuilder.ApplyConfiguration(new TarifaConfig());
    }
}