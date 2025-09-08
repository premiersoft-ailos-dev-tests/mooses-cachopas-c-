using Microsoft.EntityFrameworkCore;

namespace Bankmore.Accounts.Command.Infrastructure.Db;

public sealed class CommandDbContext : DbContext
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options) {}

    public DbSet<Entities.AccountDbModel> Contas => Set<Entities.AccountDbModel>();
    public DbSet<Entities.MovmentsDbModel> Movimentos => Set<Entities.MovmentsDbModel>();
    public DbSet<Entities.FeeDbModel> Tarifas => Set<Entities.FeeDbModel>();
    public DbSet<Entities.IdempotenciaDbModel> Idempotencias => Set<Entities.IdempotenciaDbModel>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Entities.AccountDbModel>(e =>
        {
            e.ToTable("contacorrente");

            e.Property(x => x.Numero)
                .HasColumnName("numero")
                .IsRequired();

            e.HasIndex(x => x.Numero)
                .IsUnique();

            e.Property(x => x.Nome)
                .HasColumnName("nome")
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Ativa)
                .HasColumnName("ativo")
                .IsRequired(); 

            e.Property(x => x.Senha)
                .HasColumnName("senha")
                .HasMaxLength(100)
                .IsRequired();

            e.Property(x => x.Salt)
                .HasColumnName("salt")
                .HasMaxLength(100)
                .IsRequired();
            
            e.Property(x => x.Cpf)                      
                .HasColumnName("cpf")
                .HasMaxLength(11)
                .IsRequired();
    
            e.HasIndex(x => x.Cpf).IsUnique(); 
        });

        b.Entity<Entities.MovmentsDbModel>(e =>
        {
            e.ToTable("movimento");
            e.HasKey(x => x.IdMovimento);
            e.Property(x => x.IdMovimento).HasColumnName("idmovimento");
            e.Property(x => x.IdContaCorrente).HasColumnName("idcontacorrente");
            e.Property(x => x.DataMovimento).HasColumnName("datamovimento");
            e.Property(x => x.TipoMovimento).HasColumnName("tipomovimento");
            e.Property(x => x.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)");
        });

        b.Entity<Entities.IdempotenciaDbModel>(e =>
        {
            e.ToTable("idempotencia");
            e.HasKey(x => x.ChaveIdempotencia);
            e.Property(x => x.ChaveIdempotencia).HasColumnName("chave_idempotencia");
            e.Property(x => x.Requisicao).HasColumnName("requisicao");
            e.Property(x => x.Resultado).HasColumnName("resultado");
        });
        
        b.Entity<Entities.FeeDbModel>(e =>
        {
            e.ToTable("tarifa");

            e.HasKey(x => x.IdTarifa);
            e.Property(x => x.IdTarifa).HasColumnName("idtarifa");
            e.Property(x => x.IdContaCorrente).HasColumnName("idcontacorrente");
            e.Property(x => x.DataMovimento).HasColumnName("datamovimento");
            e.Property(x => x.Valor)          .HasColumnName("valor");

            e.HasIndex(x => x.IdContaCorrente).HasDatabaseName("ix_tarifa_conta");
        });
    }
}