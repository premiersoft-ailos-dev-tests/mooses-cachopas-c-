using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.ActivateAccount;

public sealed record DeactivateAccountCommand(int NumeroConta, string Password, string Token) : IRequest<Unit>;