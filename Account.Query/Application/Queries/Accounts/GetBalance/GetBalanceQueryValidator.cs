using FluentValidation;

namespace Bankmore.Accounts.Query.Application.Queries.Accounts.GetBalance;

public sealed class GetBalanceQueryValidator : AbstractValidator<GetBalanceQuery>
{
    public GetBalanceQueryValidator()
    {
        RuleFor(x => x.numeroConta)
            .GreaterThan(0).WithMessage("AccountId é obrigatório.");
    }
}