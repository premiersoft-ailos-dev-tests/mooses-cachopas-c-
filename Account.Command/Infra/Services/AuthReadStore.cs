using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace Bankmore.Accounts.Command.Infrastructure.Services;

public sealed class AuthReadStore : IAuthReadStore
{
    private readonly CommandDbContext _db;
    public AuthReadStore(CommandDbContext db) => _db = db;

    public async Task<(bool found, string accountId, string senhaHash, string salt)>
        GetCredentialsByNumeroAsync(int numero, CancellationToken ct)
    {
        var x = await _db.Contas.AsNoTracking()
            .Where(c => c.Numero == numero && c.Ativa)
            .Select(c => new { c.Numero, c.Senha, c.Salt })
            .FirstOrDefaultAsync(ct);

        return x is null ? (false, "", "", "") : (true, x.Numero.ToString(), x.Senha, x.Salt);
    }

    public async Task<(bool found, string accountId, string senhaHash, string salt)>
        GetCredentialsByCpfAsync(string cpfDigits, CancellationToken ct)
    {
        var x = await _db.Contas.AsNoTracking()
            .Where(c => c.Cpf == cpfDigits && c.Ativa)
            .Select(c => new { c.Numero, c.Senha, c.Salt })
            .FirstOrDefaultAsync(ct);

        return x is null ? (false, "", "", "") : (true, x.Numero.ToString(), x.Senha, x.Salt);
    }
}