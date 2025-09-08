using Bankmore.Accounts.Command.Domain.Accounts;

namespace Bankmore.Accounts.Command.Application.Abstractions;

public interface IAccountsStore
{
    Task<CreateAccountModel> CreateAccountAsync(CreateAccountModel contaCorrente, CancellationToken ct);
    Task<bool> CpfExistsAsync(string cpf, CancellationToken ct);
    Task<int?> GetAccountIdByCpfAsync(string cpfDigits, CancellationToken ct);
    Task<(int Numero, bool Ativo, string Senha, string Salt)?> GetAccountWithCredentialsAsync(int numeroConta, CancellationToken ct);
    Task SetAccountActiveAsync(int numeroConta, bool active, CancellationToken ct);
    
    Task<AccountValid> GetAccountAsync(int numeroConta, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
