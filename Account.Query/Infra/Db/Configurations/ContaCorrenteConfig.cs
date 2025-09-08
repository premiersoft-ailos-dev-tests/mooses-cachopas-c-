using Bankmore.Accounts.Query.Infrastructure.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bankmore.Accounts.Query.Infrastructure.Db.Configurations;

public sealed class ContaCorrenteConfig : IEntityTypeConfiguration<ContaCorrente>
{
    public void Configure(EntityTypeBuilder<ContaCorrente> b)
    {
        b.ToTable("contacorrente");
        b.HasKey(x => x.IdContaCorrente);
        b.Property(x => x.IdContaCorrente).HasColumnName("idcontacorrente");
        b.Property(x => x.Numero).HasColumnName("numero");
        b.Property(x => x.Nome).HasColumnName("nome");
        b.Property(x => x.Ativo).HasColumnName("ativo"); 
    }
}