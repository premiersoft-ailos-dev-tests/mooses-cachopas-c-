using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Common.Exceptions;
using Bankmore.Accounts.Command.Domain.Accounts;
using MediatR;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;

public sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CreateAccountResult>
{
    private readonly IAccountsService _service;
    private readonly IPasswordHasher _hasher;

    public CreateAccountCommandHandler(IAccountsService store, IPasswordHasher hasher)
    {
        _service = store; _hasher = hasher;
    }

    public async Task<CreateAccountResult> Handle(CreateAccountCommand req, CancellationToken ct)
    {
        try
        {

            if (!CpfUtils.IsValid(req.Cpf))
                throw new BusinessRuleException("CPF inválido.", "INVALID_DOCUMENT");

            if (await _service.CpfExistsAsync(req.Cpf, ct))
                throw new BusinessRuleException("CPF já cadastrado.");
            var (hash, salt) = _hasher.HashPassword(req.Senha);

            Guid accountId = Guid.NewGuid();
            var acc = CreateAccountModel.Create(req.Nome, req.Cpf, hash, salt);

            var result = await _service.CreateAccountAsync(acc, ct);

            return new CreateAccountResult(result.Numero);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}