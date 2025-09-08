using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Commands.Accounts.ActivateAccount;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.DeactivateAccount;

public sealed class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand, Unit>
{
    private readonly IAccountsStore _store;
    private readonly IPasswordHasher _hasher;

    public DeactivateAccountCommandHandler(IAccountsStore store, IPasswordHasher hasher)
    {
        _store = store;
        _hasher = hasher;
    }

    public async Task<Unit> Handle(DeactivateAccountCommand req, CancellationToken ct)
    {
        var acc = await _store.GetAccountWithCredentialsAsync(req.NumeroConta, ct);
        if (acc is null || !acc.Value.Ativo)
            throw new BusinessRuleException("Account not found or already inactive.") 
                { HResult = 400, Source = "INVALID_ACCOUNT" };

        if (!_hasher.Verify(req.Password, acc.Value.Senha, acc.Value.Salt))
            throw new UnauthorizedAccessException("Invalid password.");

        await _store.SetAccountActiveAsync(req.NumeroConta, false , ct);
        await _store.SaveChangesAsync(ct);

        return Unit.Value;
    }
}