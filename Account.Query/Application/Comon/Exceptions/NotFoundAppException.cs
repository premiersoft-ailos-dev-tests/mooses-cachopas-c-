namespace Bankmore.Accounts.Query.Application.Comon.Exceptions;

public sealed class NotFoundAppException : Exception
{
    public NotFoundAppException(string message) : base(message) { }
}