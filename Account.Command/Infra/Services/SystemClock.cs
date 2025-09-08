using Bankmore.Accounts.Command.Application.Abstractions;

namespace Bankmore.Accounts.Command.Infrastructure.Services;
public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }