using Bankmore.Accounts.Command.Domain.Enums.Movments;

namespace Bankmore.Accounts.Command.Domain.Extensions.Movments;

public static class MovmentTypeExtensions
{
    public static MovmentType ToMovmentType(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return MovmentType.Default;

        return value.Trim().ToUpper() switch
        {
            "C" => MovmentType.Credit,
            "D" => MovmentType.Debit,
            _   => MovmentType.Default
        };
    }
    
    public static string ToCode(this MovmentType movmentType)
    {
        return movmentType switch
        {
            MovmentType.Credit => "C",
            MovmentType.Debit  => "D",
            _ => string.Empty
        };
    }
}