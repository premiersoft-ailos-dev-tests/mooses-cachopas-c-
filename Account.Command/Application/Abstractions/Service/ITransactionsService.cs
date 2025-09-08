using Bankmore.Accounts.Command.Domain.Models.Movments;

namespace Bankmore.Accounts.Command.Application.Abstractions;

public interface ITransactionsService
{
    Task<bool> RegisterMovment(MovmentModel movmentModel, CancellationToken ct);

    Task<decimal> GetSaldoAtualAsync(int numeroConta, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}