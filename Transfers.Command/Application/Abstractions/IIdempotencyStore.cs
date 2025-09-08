namespace Application.Abstractions;

public interface IIdempotencyStore
{
    Task<string?> GetResultAsync(string key, CancellationToken ct);
    Task SaveAsync(string key, string requestJson, string resultJson, CancellationToken ct);
}