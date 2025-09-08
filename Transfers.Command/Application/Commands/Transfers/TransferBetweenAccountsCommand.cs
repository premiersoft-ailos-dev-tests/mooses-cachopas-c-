using MediatR;

namespace Application.Commands.Transfers;

public sealed record TransferBetweenAccountsCommand(
    int OrigemAccountId,
    int DestinoAccountId,
    decimal Valor,
    string IdempotencyKey,
    string Token
) : IRequest<TransferBetweenAccountsResult>;

public sealed record TransferBetweenAccountsResult(
    bool Success,
    string? ErrorType = null,
    string? ErrorMessage = null
);