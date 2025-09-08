using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.DeactivateAccount;

public sealed record ActivateAccountCommand(int NumeroConta, string Password, string Token) : IRequest<Unit>;