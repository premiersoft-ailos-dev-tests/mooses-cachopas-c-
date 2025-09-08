using Bankmore.Accounts.Query.Application.Abstraction;
using Bankmore.Accounts.Query.Application.Comon.Exceptions;
using MediatR;

namespace Bankmore.Accounts.Query.Application.Queries.Accounts.GetBalance;

public sealed class GetBalanceQueryHandler
    : IRequestHandler<GetBalanceQuery, GetBalanceResult>
{
    private readonly IAccountsReadStore _readStore;

    public GetBalanceQueryHandler(IAccountsReadStore readStore)
        => _readStore = readStore;

    public async Task<GetBalanceResult> Handle(GetBalanceQuery req, CancellationToken ct)
    {
        var acc = await _readStore.GetAccountAsync(req.numeroConta, ct);
        if (acc is null)
            throw new NotFoundAppException("Conta não encontrada.");
        if (!acc.Activa)
            throw new ForbiddenAppException("Conta inativa.");
        
        var bal = await _readStore.GetBalanceAsync(req.numeroConta, req.IncludeTariffs, ct);
        if (bal is null)
            throw new NotFoundAppException("Saldo não encontrado.");

        return new GetBalanceResult(
            bal.AvailableBalance,
            acc.Nome,
            acc.NumeroConta,
            bal.AsOfUtc
        );
    }
}