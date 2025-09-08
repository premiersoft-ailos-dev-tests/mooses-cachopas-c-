using Application.Abstractions;
using Infra.Db;
using Infra.Db.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCommandInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CommandDbContext>(opt =>
            opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        services.AddScoped<ITransfersStore, TransfersStore>();

        return services;
    }
}