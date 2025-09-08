using FluentValidation;

namespace Application.Commands.Transfers;

public sealed class TransferBetweenAccountsCommandValidator
    : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.OrigemAccountId)
            .NotEmpty().WithMessage("OrigemAccountId é obrigatório.");

        RuleFor(x => x.DestinoAccountId)
            .NotEmpty().WithMessage("DestinoAccountId é obrigatório.")
            .NotEqual(x => x.OrigemAccountId).WithMessage("DestinoAccountId não pode ser igual à OrigemAccountId.");

        RuleFor(x => x.Valor)
            .GreaterThan(0m).WithMessage("Apenas valores positivos são permitidos.")
            .WithState(_ => "INVALID_VALUE"); 

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Cabeçalho Idempotency-Key é obrigatório.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Cabeçalho Authorization é obrigatório."); 
    }
}