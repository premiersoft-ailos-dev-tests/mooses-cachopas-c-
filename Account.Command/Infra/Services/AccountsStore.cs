using AutoMapper;
using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Domain.Accounts;
using Bankmore.Accounts.Command.Infrastructure.Db;
using Bankmore.Accounts.Command.Infrastructure.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bankmore.Transfers.Command.Infrastructure.Services;

public sealed class AccountsStore : IAccountsStore
{
    private readonly CommandDbContext _db;
    private readonly IMapper _mapper;

    public AccountsStore(CommandDbContext db,IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    } 

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    public async Task<AccountDbModel> CreateAccountAsync(AccountDbModel conta, CancellationToken ct)
    {
        _db.Contas.Add(conta);
        await _db.SaveChangesAsync(ct);
        return conta;
    }

    public async Task<CreateAccountModel> CreateAccountAsync(CreateAccountModel contaCorrente, CancellationToken ct)
    {
        var account = _mapper.Map<CreateAccountModel, AccountDbModel>(contaCorrente);
        _db.Contas.Add(account);
        await _db.SaveChangesAsync(ct);
        return _mapper.Map<AccountDbModel,CreateAccountModel>(account);
    }

    public async Task<bool> CpfExistsAsync(string cpf, CancellationToken ct)
    {
        var onlyDigits = new string(cpf.Where(char.IsDigit).ToArray());

        return await _db.Contas
            .AsNoTracking()
            .AnyAsync(c => c.Cpf == onlyDigits, ct);
    }

    public async Task<(int Numero, bool Ativo, string Senha, string Salt)?> GetAccountWithCredentialsAsync(int NumeroConta, CancellationToken ct)
    {
        var acc = await _db.Contas.AsNoTracking()
            .Where(c => c.Numero == NumeroConta)
            .Select(c => new { c.Numero, Ativo = c.Ativa, c.Senha, c.Salt })
            .FirstOrDefaultAsync(ct);

        return acc is null ? null : (acc.Numero, acc.Ativo, acc.Senha, acc.Salt);
    }

    public async Task SetAccountActiveAsync(int numeroConta, bool active, CancellationToken ct)
    {
        var acc = await _db.Contas.FindAsync(new object?[] { numeroConta }, ct);
        if (acc is not null)
        {
            acc.Ativa = active;
            _db.Contas.Update(acc);
        }
    }

    public async Task<AccountValid> GetAccountAsync(int numeroConta, CancellationToken ct)
{
    return await _db.Contas
        .AsNoTracking()
        .Where(c => c.Numero == numeroConta)
        .Select(c => new AccountValid(true, c.Numero, c.Nome, c.Ativa))
        .FirstOrDefaultAsync(ct);
}

    public async Task<int?> GetAccountIdByCpfAsync(string cpfDigits, CancellationToken ct)
    {
        var only = new string(cpfDigits.Where(char.IsDigit).ToArray());
        return await _db.Contas.AsNoTracking()
            .Where(c => c.Cpf == only)
            .Select(c => c.Numero)
            .FirstOrDefaultAsync(ct);
    }
}