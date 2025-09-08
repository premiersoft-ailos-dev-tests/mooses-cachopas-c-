using System.Text.Json;
using Infra.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Infra.Movments;

public static class StartupMovmentsInfrastructure
{
    public static IServiceCollection AddMovmentsInfra(this IServiceCollection services,
        ConfigurationManager configurationManager)
    {
        var movsConfig = configurationManager
            .GetRequiredSection("Apis:Movments")
            .Get<MovmentsApiSettings>();

        services.AddRefitClient<IMovmentsApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(movsConfig!.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(movsConfig.TimeoutSeconds <= 0 ? 10 : movsConfig.TimeoutSeconds);
            });

        services.AddScoped<IMovmentsGateway, MovmentsGateway>();

        return services;
    }
}