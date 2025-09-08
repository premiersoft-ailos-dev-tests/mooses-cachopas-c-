using Bankmore.Accounts.Query.Application.Comon.Models;

namespace Bankmore.Accounts.Query.Application.Abstraction;

public interface IAccountsReadStore
{
    Task<AccountInfo?> GetAccountAsync(int numeroConta, CancellationToken ct);
    
    Task<BalanceProjection?> GetBalanceAsync(
        int numeroConta,
        bool includeTariffs,               
        CancellationToken ct);
}

public sealed class AccountInfo
{
    public int NumeroConta { get; init; }                     
    public string Nome { get; init; } = default!;        
    public bool Activa { get; init; }                  
}