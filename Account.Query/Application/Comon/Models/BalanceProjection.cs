namespace Bankmore.Accounts.Query.Application.Comon.Models;
public sealed class BalanceProjection
{
    public string AccountId { get; init; } = default!;
    public decimal LedgerBalance { get; init; }          
    public decimal AvailableBalance { get; init; }       
    public string Currency { get; init; } = "BRL";
    public long Version { get; init; }                   
    public DateTime AsOfUtc { get; init; }               
}