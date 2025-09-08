namespace Bankmore.Accounts.Command.Domain.Accounts;

public readonly record struct Money(decimal Value)
{
    public static Money Zero => new(0m);
    public static Money operator +(Money a, Money b) => new(a.Value + b.Value);
    public static Money operator -(Money a, Money b) => new(a.Value - b.Value);
    public static implicit operator decimal(Money m) => m.Value;
}