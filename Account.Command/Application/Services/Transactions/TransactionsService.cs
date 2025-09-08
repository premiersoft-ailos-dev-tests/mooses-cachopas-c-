using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Domain.Models.Movments;

namespace Bankmore.Accounts.Command.Application.Commands.Services.Transactions;

public class TransactionsService : ITransactionsService
{
    private readonly ITransactionsStore _store;

    public TransactionsService(ITransactionsStore store)
    {
        _store = store;
    }
    
    public Task<bool> RegisterMovment(MovmentModel movmentModel, CancellationToken ct)
    {
        return _store.RegisterMovment(movmentModel, ct);
    }

    public Task AddMovimentoAsync(MovmentModel movmentModel, CancellationToken ct)
    {
        return _store.RegisterMovment(movmentModel, ct);
    }

    public Task<decimal> GetSaldoAtualAsync(int numeroConta, CancellationToken ct)
    {
        return _store.GetSaldoAtualAsync(numeroConta, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _store.SaveChangesAsync(ct);
    }
}