namespace Bankmore.Accounts.Query.Application.Comon.Exceptions;

public sealed class ForbiddenAppException : Exception
{
    public ForbiddenAppException(string? message = null)
        : base(message ?? "Forbidden") { }
}