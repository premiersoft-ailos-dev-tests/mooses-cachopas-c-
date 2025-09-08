using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.DeactivateAccount;

public sealed class ActivateAccountHandler : IRequestHandler<ActivateAccountCommand, Unit>
{
    private readonly IAccountsService _store;
    private readonly IPasswordHasher _hasher;

    public ActivateAccountHandler(IAccountsService store, IPasswordHasher hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public async Task<Unit> Handle(ActivateAccountCommand req, CancellationToken ct)
    {

        var acc = await _store.GetAccountWithCredentialsAsync(req.NumeroConta, ct);
        if (acc is null)
            throw new BusinessRuleException("Account not found.") { HResult = 400, Source = "INVALID_ACCOUNT" };

        if (acc.Value.Ativo)
            throw new BusinessRuleException("Account is already active.") { HResult = 400, Source = "ALREADY_ACTIVE" };

        if (!_hasher.Verify(req.Password, acc.Value.Senha, acc.Value.Salt))
            throw new UnauthorizedAccessException("Invalid password.");

        await _store.SetAccountActiveAsync(req.NumeroConta, true, ct);
        await _store.SaveChangesAsync(ct);

        return Unit.Value;
    }
}