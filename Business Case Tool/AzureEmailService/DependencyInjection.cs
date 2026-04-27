using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BCT.Application.ServiceInterfaces;

namespace BCT.AzureEmailService;
public static class DependencyInjection
{
    public static IServiceCollection AddAzureEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        //Reading the configuration from a JSON file
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        var json = File.ReadAllText(configPath);
        var config = System.Text.Json.JsonSerializer.Deserialize<AzureEmailServiceConfig>(json);
        AzureEmailService.EmailLogicAppUrl = config?.EmailLogicAppUrl ?? throw new InvalidOperationException("EmailLogicAppUrl is not configured.");

        services.AddSingleton<IEmailService, AzureEmailService>();
        return services;
    }

    private record AzureEmailServiceConfig(string EmailLogicAppUrl);
}