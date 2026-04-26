using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BCT.Auth0Api;
using BCT.EF;
using BCT.Application;
using BCT.ExcelConverter;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using BCT.Domain;
using BCT.HubSpotIntegration;
using BCT.Logging;
using BCT.AzureEmailService;
namespace BCT.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
        //Plugins
        services
            .AddEF(configuration)
            .AddAuth0Api(configuration)
            .AddExcelConverter(configuration)
            .AddApp(configuration)
            .AddHupSpotIntergration(configuration)
            .AddEventLogger(configuration)
            .AddAzureEmailService(configuration);

        //Domain calculation service
        services.AddSingleton<ICalculation, Calculation>();

		services.AddHangfire(c => c
			.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
			.UseSimpleAssemblyNameTypeSerializer()
			.UseDefaultTypeResolver()
			.UseDefaultTypeSerializer()
			.UseInMemoryStorage());

		services.AddHangfireServer();

		return services;
	}

	public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
	{
#if DEBUG
		app.UseHangfireDashboard();
#endif
		return app;
	}
}
