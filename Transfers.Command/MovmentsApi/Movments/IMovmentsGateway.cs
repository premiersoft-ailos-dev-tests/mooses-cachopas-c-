using Infra.Contracts;
using Infra.Models;

namespace Infra.Movments;

public interface IMovmentsGateway
{
    Task MovmentAsync(
        string bearerToken,       
        string idempotencyKey,
        int? numeroConta,         
        decimal valor,
        string tipoOperacao,      
        CancellationToken ct);
}

public sealed class MovmentsGateway : IMovmentsGateway
{
    private readonly IMovmentsApi _api;

    public MovmentsGateway(IMovmentsApi api) => _api = api;

    public async Task MovmentAsync(
        string bearerToken,
        string idempotencyKey,
        int? numeroConta,
        decimal valor,
        string tipoOperacao,
        CancellationToken ct)
    {
        var authHeader = $"Bearer {bearerToken}";
        var idempHeader = idempotencyKey;
        var body = new MovmentRequest(numeroConta ?? 0, valor, tipoOperacao);

        var res = await _api.MovmentAsync(body,authHeader,idempHeader, ct);

        if (res.IsSuccessStatusCode) return;

        if ((int)res.StatusCode == 403)
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        var msg = string.IsNullOrWhiteSpace(res.Content) ? "Falha na Movments API." : res.Content;
        throw new InvalidOperationException(msg);
    }
}