using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;

public sealed record CreateAccountCommand(
    string Nome,
    string Senha,
    string Cpf
) : IRequest<CreateAccountResult>;

public sealed record CreateAccountResult(
    int Numero
    );