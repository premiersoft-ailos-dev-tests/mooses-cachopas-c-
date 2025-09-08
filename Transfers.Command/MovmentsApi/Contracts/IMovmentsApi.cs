using Infra.Models;
using Refit;

namespace Infra.Contracts;
public interface IMovmentsApi
{
    [Post("/v1/contas/movimento")]
    Task<ApiResponse<string>> MovmentAsync(
        [Body] MovmentRequest body,
        [Header("Authorization")] string authorization,
        [Header("X-Idempotency-Key")] string idempotencyKey,
        CancellationToken ct);

    [Post("/v1/auth/login")]
    Task<ApiResponse<LoginResponse>> LoginAsync(
        [Body] LoginRequest body,
        CancellationToken ct);
}
