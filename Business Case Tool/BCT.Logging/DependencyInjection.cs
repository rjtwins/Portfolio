using BCT.Application.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCT.Logging;
public static class DependencyInjection
{
    public static IServiceCollection AddEventLogger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventLogger, EventLogger>();
        return services;
    }
}