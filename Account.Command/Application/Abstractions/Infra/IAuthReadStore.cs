namespace Bankmore.Accounts.Command.Application.Abstractions;

public interface IAuthReadStore
{
    Task<(bool found, string accountId, string senhaHash, string salt)> GetCredentialsByNumeroAsync(int numero, CancellationToken ct);
    Task<(bool found, string accountId, string senhaHash, string salt)> GetCredentialsByCpfAsync(string cpfDigits, CancellationToken ct);
}