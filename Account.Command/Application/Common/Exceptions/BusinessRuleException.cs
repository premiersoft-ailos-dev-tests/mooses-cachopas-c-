namespace Bankmore.Accounts.Command.Application.Common.Exceptions;

public class BusinessRuleException : Exception
{
    public string ErrorType { get; }

    public BusinessRuleException(string message, string errorType = "BUSINESS_ERROR")
        : base(message)
    {
        ErrorType = errorType;
    }
}