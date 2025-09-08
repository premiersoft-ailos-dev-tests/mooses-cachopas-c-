using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Domain.Accounts;

namespace Bankmore.Accounts.Command.Application.Commands.Services.Accounts;

public class AccountsService : IAccountsService
{
    private readonly IAccountsStore _store;

    public AccountsService(IAccountsStore store)
    {
        _store = store;
    }

    public Task<bool> CpfExistsAsync(string cpf, CancellationToken ct)
    {
        return _store.CpfExistsAsync(cpf, ct);
    }

    public Task<CreateAccountModel> CreateAccountAsync(CreateAccountModel accountModel, CancellationToken ct)
    {
        return _store.CreateAccountAsync(accountModel, ct); ;
    }
    
    public Task<AccountValid> GetAccountAsync(int accountId, CancellationToken ct)
    {
        return _store.GetAccountAsync(accountId, ct);
    }

    public Task<int?> GetAccountIdByCpfAsync(string cpfDigits, CancellationToken ct)
    {
        return _store.GetAccountIdByCpfAsync(cpfDigits, ct);
    }

    public Task<(int Numero, bool Ativo, string Senha, string Salt)?> GetAccountWithCredentialsAsync(int NumeroConta, CancellationToken ct)
    {
        return _store.GetAccountWithCredentialsAsync(NumeroConta, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _store.SaveChangesAsync(ct);
    }

    public Task SetAccountActiveAsync(int numeroConta, bool active, CancellationToken ct)
    {
        return _store.SetAccountActiveAsync(numeroConta, active, ct);
    }
}