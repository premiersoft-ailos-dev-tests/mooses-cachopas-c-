using Infra.Movments;

namespace Application.Services;

public class TransferAppService
{
    private readonly IMovmentsGateway _movs;

    public TransferAppService(IMovmentsGateway movs) => _movs = movs;

    public Task DebitarOrigemAsync(string token, string requestId, decimal valor, CancellationToken ct)
        => _movs.MovmentAsync(token, requestId, null, valor, "D", ct);

    public Task CreditarDestinoAsync(string token, string requestId, int numeroContaDestino, decimal valor, CancellationToken ct)
        => _movs.MovmentAsync(token, requestId, numeroContaDestino, valor, "C", ct);
}