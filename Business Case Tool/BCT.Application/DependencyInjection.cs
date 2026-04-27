using BCT.Application.EventManagement;
using BCT.Application.Services;
using BCT.Application.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCT.Application;
public static class DependencyInjection
{
	public static IServiceCollection AddApp(this IServiceCollection services, IConfiguration configuration)
	{
		//Application state services:
		services.AddSingleton<AuthToken>();
		
		services.AddSingleton<UserAuthenticatedNotifier, UserAuthenticatedNotifier>();
		services.AddSingleton<NewCompanyNotifier, NewCompanyNotifier>();
		services.AddSingleton<NewProjectNotifier, NewProjectNotifier>();
		services.AddSingleton<ProjectContentUpdatedNotifier, ProjectContentUpdatedNotifier>();
		services.AddSingleton<CompanyContentUpdatedNotifier, CompanyContentUpdatedNotifier>();
		services.AddSingleton<ProjectRemovedNotifier, ProjectRemovedNotifier>();
		services.AddSingleton<CompanyRemovedNotifier, CompanyRemovedNotifier>();
        services.AddSingleton<UserLoginNotifier, UserLoginNotifier>();
        services.AddSingleton<UserLogoutNotifier, UserLogoutNotifier>();
        services.AddSingleton<ICacheRegistry, CacheRegistry>();

        //Services:
        services.AddSingleton<UserSyncService>();

		//Use cases:
		RegisterAllUseCases(services);

		return services;
	}

	private static void RegisterAllUseCases(IServiceCollection services)
	{
		var useCaseInterfaceType = typeof(IUseCase);
		var useCaseTypes = useCaseInterfaceType.Assembly.GetTypes()
			.Where(t => t.GetInterfaces().Contains(useCaseInterfaceType) && t.IsClass);

		foreach (var useCaseType in useCaseTypes)
		{
			var interfaceType = useCaseType.GetInterfaces().FirstOrDefault(i => i != useCaseInterfaceType);
			if (interfaceType != null)
			{
				services.AddTransient(interfaceType, useCaseType);
			}
		}
	}
}
