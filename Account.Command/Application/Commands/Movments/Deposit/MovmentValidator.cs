using FluentValidation;

namespace Bankmore.Accounts.Command.Application.Commands.Transactions.Movments;

public sealed class MovmentValidator : AbstractValidator<MovmentCommand>
{
    public MovmentValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor da operação deve ser maior que zero.");
        RuleFor(x => x.MovmentType)
            .Equal(Domain.Enums.Movments.MovmentType.Default).WithMessage("Tipo de movimentação inválida para depósito.");
    }
}