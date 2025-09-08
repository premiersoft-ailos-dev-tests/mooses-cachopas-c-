using Bankmore.Accounts.Command.Domain.Enums.Movments;
using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Transactions.Movments;

public sealed record MovmentCommand
(
    int? IdConta,
    decimal Valor,
    string Token,
    MovmentType MovmentType,
    string IdempotencyKey
) : IRequest<MovmentResult>;

public sealed record MovmentResult(
    bool Success,
    string? ErrorType = null,
    string? ErrorMessage = null
);