namespace Bankmore.Accounts.Command.Infrastructure.Db.Entities;

public sealed class FeeDbModel
{
    public string IdTarifa { get; set; } = default!;
    public int IdContaCorrente { get; set; }
    public DateTime DataMovimento { get; set; } = default!;
    public decimal Valor { get; set; }
}