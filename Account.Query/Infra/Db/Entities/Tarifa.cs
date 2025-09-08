namespace Bankmore.Accounts.Query.Infrastructure.Db.Entities;

public sealed class Tarifa
{
    public string IdTarifa { get; set; } = default!;
    public int NumeroConta { get; set; }
    public DateTime DataMovimento { get; set; } = default!;    
    public decimal Valor { get; set; }                      
}