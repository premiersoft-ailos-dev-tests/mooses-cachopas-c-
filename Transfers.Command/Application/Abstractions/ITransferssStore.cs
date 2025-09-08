namespace Application.Abstractions;

public interface ITransfersStore
{
    Task<int> AddTransferAsync(int origemId, int destinoId, DateTime dataMov, decimal valor, CancellationToken ct);
    Task<AccountValid> GetAccountAsync(int numeroConta, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}