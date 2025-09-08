using MediatR;

namespace Bankmore.Accounts.Query.Application.Queries.Accounts.GetBalance;

public sealed record GetBalanceQuery(
    int numeroConta,
    string Token,          
    bool IncludeTariffs = true
) : IRequest<GetBalanceResult>;