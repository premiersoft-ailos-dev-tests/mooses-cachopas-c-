using Bankmore.Accounts.Query.Infrastructure.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bankmore.Accounts.Query.Infrastructure.Db.Configurations;

public sealed class TarifaConfig : IEntityTypeConfiguration<Tarifa>
{
    public void Configure(EntityTypeBuilder<Tarifa> b)
    {
        b.ToTable("tarifa");
        b.HasKey(x => x.IdTarifa);
        b.Property(x => x.IdTarifa).HasColumnName("idtarifa");
        b.Property(x => x.NumeroConta).HasColumnName("idcontacorrente");
        b.Property(x => x.DataMovimento).HasColumnName("datamovimento");
        b.Property(x => x.Valor).HasColumnName("valor").HasColumnType("decimal(18,2)");
    }
}