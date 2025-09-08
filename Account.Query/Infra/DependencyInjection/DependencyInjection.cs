
using Bankmore.Accounts.Query.Application.Abstraction;
using Bankmore.Accounts.Query.Infrastructure.Db;
using Bankmore.Accounts.Query.Infrastructure.ReadStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bankmore.Accounts.Query.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddQueryInfrastructure(
        this IServiceCollection services,
        string connectionString,
        bool useMySql = true)
    {
        if (useMySql)
        {
            services.AddDbContext<AccountsQueryDbContext>(opt =>
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        }

        services.AddScoped<IAccountsReadStore, AccountsReadStore>();

        return services;
    }
}