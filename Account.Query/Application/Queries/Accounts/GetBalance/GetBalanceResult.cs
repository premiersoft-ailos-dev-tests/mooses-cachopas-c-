namespace Bankmore.Accounts.Query.Application.Queries.Accounts.GetBalance;

public sealed record GetBalanceResult(
    decimal SaldoDisponivel,
    string Nome,
    int NumerConta,
    DateTime AsOfUtc
);