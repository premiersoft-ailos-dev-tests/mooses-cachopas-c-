namespace Bankmore.Accounts.Query.Infrastructure.Db.Entities;

public sealed class ContaCorrente
{
    public string IdContaCorrente { get; set; } = default!;  
    public int Numero { get; set; }                          
    public string Nome { get; set; } = default!;             
    public bool Ativo { get; set; }                          
}