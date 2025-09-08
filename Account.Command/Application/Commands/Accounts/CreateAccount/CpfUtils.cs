using System.Text.RegularExpressions;

namespace Bankmore.Accounts.Command.Application.Commands.Accounts.CreateAccount;

internal static class CpfUtils
{
    public static bool IsValid(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        var digits = Regex.Replace(cpf, "[^0-9]", "");
        if (digits.Length != 11) return false;
        
        if (new string(digits[0], 11) == digits) return false;
        
        int Calc(string src, int length)
        {
            int sum = 0;
            int weight = length + 1;
            for (int i = 0; i < length; i++)
                sum += (src[i] - '0') * (weight--);
            int mod = sum % 11;
            return mod < 2 ? 0 : 11 - mod;
        }

        var d1 = Calc(digits, 9);
        if (digits[9] - '0' != d1) return false;

        var d2 = Calc(digits, 10);
        if (digits[10] - '0' != d2) return false;

        return true;
    }
}