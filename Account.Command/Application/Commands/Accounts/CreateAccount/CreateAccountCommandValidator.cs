using Bankmore.Accounts.Command.Application.Abstractions;
using FluentValidation;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator(IAccountsStore store)
    {
        
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório.")
            .Must(CpfUtils.IsValid)
            .WithMessage("CPF inválido.")
            .WithState(_ => "INVALID_DOCUMENT");
    }
}