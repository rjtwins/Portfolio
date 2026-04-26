using BCT.Application.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCT.HubSpotIntegration;
public static class DependencyInjection
{
    public static IServiceCollection AddHupSpotIntergration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHupSpotIntegrationService, HubSpotIntegrationService>();
        return services;
    }
}
