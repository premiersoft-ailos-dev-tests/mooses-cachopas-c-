using Bankmore.Accounts.Command.Application.Abstractions;
using Bankmore.Accounts.Command.Application.Commands.Services.Accounts;
using Bankmore.Accounts.Command.Application.Commands.Services.Transactions;
using Bankmore.Accounts.Command.Infrastructure.Db;
using Bankmore.Accounts.Command.Infrastructure.Services;
using Bankmore.Transfers.Command.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bankmore.Accounts.Command.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommandInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CommandDbContext>(opt =>
            opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        
        services.AddScoped<IAccountsStore, AccountsStore>();
        services.AddScoped<IAuthReadStore, AuthReadStore>();

        services.AddScoped<ITransactionsService, TransactionsService>();
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<ITransactionsStore, TransactionsStore>();

        return services;
    }
}