using Microsoft.EntityFrameworkCore;

namespace Infra.Db;

public sealed class CommandDbContext : DbContext
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options) {}

    public DbSet<Entities.AccountDbModel> Contas => Set<Entities.AccountDbModel>();
    public DbSet<Entities.TransfersDbModel> Transferencias => Set<Entities.TransfersDbModel>();
    public DbSet<Entities.IdempotenciaDbModel> Idempotencias => Set<Entities.IdempotenciaDbModel>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Entities.TransfersDbModel>(e =>
        {
            e.ToTable("transferencia");
            e.HasKey(x => x.IdTransferencia);
            e.Property(x => x.IdTransferencia).HasColumnName("idtransferencia");
            e.Property(x => x.IdContaCorrenteOrigem).HasColumnName("idcontacorrente_origem");
            e.Property(x => x.IdContaCorrenteDestino).HasColumnName("idcontacorrente_destino");
            e.Property(x => x.DataMovimento)
                .HasColumnName("datamovimento")
                .HasColumnType("DATE");
            e.Property(x => x.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)");
        });
        
        b.Entity<Entities.AccountDbModel>(e =>
        {
            e.ToTable("contacorrente");
            e.Property(x => x.Numero).HasColumnName("numero");
            e.HasIndex(x => x.Numero).IsUnique();
            e.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(100);
            e.Property(x => x.Ativa).HasColumnName("ativo"); 
        });

        b.Entity<Entities.IdempotenciaDbModel>(e =>
        {
            e.ToTable("idempotencia");
            e.HasKey(x => x.ChaveIdempotencia);
            e.Property(x => x.ChaveIdempotencia).HasColumnName("chave_idempotencia");
            e.Property(x => x.Requisicao).HasColumnName("requisicao");
            e.Property(x => x.Resultado).HasColumnName("resultado");
        });
    }
}