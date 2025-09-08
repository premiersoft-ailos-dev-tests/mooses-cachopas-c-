namespace Bankmore.Accounts.Command.Application.Abstractions;

public interface IClock { DateTime UtcNow { get; } }