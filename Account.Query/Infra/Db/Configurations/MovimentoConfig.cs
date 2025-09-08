using Bankmore.Accounts.Query.Infrastructure.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bankmore.Accounts.Query.Infrastructure.Db.Configurations;

public sealed class MovimentoConfig : IEntityTypeConfiguration<Movimento>
{
    public void Configure(EntityTypeBuilder<Movimento> b)
    {
        b.ToTable("movimento");
        b.HasKey(x => x.IdMovimento);
        b.Property(x => x.IdMovimento).HasColumnName("idmovimento");
        b.Property(x => x.NumeroConta).HasColumnName("idcontacorrente");
        b.Property(x => x.DataMovimento).HasColumnName("datamovimento");
        b.Property(x => x.TipoMovimento).HasColumnName("tipomovimento");
        b.Property(x => x.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)");
    }
}