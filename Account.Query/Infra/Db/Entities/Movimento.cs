namespace Bankmore.Accounts.Query.Infrastructure.Db.Entities;

public sealed class Movimento
{
    public int IdMovimento { get; set; }
    public int NumeroConta { get; set; }
    public DateTime DataMovimento { get; set; } = default!;    
    public string TipoMovimento { get; set; } = default!;    
    public decimal Valor { get; set; }                      
}