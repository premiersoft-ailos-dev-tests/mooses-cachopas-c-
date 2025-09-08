namespace Bankmore.Accounts.Query.Api.Contracts;

public sealed class BalanceResponse
{
    public decimal SaldoDisponivel { get; set; }
    public int NumeroDaConta { get; set; }
    public string Nome { get; set; }
    public DateTime AsOfUtc { get; set; }
}