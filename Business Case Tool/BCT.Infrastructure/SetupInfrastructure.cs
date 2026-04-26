using BCT.Application.ServiceInterfaces;
using BCT.EF;
using Microsoft.Extensions.DependencyInjection;
using BCT.Application.AuthEntities;
using BCT.Application.Services;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using BCT.Application.UseCases.Commands;

namespace BCT.Infrastructure;

public static class Setup
{
	public static async Task SetupInfrastructure(IServiceProvider serviceProvider)
	{
		//SetupDB
		var contextFactory = serviceProvider.GetService<IDbContextFactory<ApplicationDbContext>>();
		using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

        //Initial login to Auth0 Management API
        var auth0ManagementApi = serviceProvider.GetService<IAuthManagementService>();
		var auth0Token = serviceProvider.GetService<AuthToken>();
		var freshToken = await auth0ManagementApi.GetAuth0ManagementApiToken();		
		auth0Token.Refresh(freshToken);
		
		//Start the token refresh service
		var auth0ManagementTokenRefreshService = serviceProvider.GetService<IAuth0ManagementTokenRefreshService>();

        //Initial check.
        auth0ManagementTokenRefreshService.CheckAndRefresh();
        //Using hangfire to setup an hourly check and refresh of the token.
        RecurringJob.AddOrUpdate<IAuth0ManagementTokenRefreshService>("TokenRefreshJob", (s) => s.CheckAndRefresh(), Cron.Minutely);
		
		//Subscribe to user auth events events:
		var userSyncService = serviceProvider.GetService<UserSyncService>();
		userSyncService.Start();
		await userSyncService.SyncUsers();

        //Fix missing values after migrations:
        var processMissingValueMigrationsUseCase = serviceProvider.GetService<IProcessMissingValueMigrationsUseCase>();
        processMissingValueMigrationsUseCase.Execute();

        //Make sure Hupspot service is started.
        var hupSpotIntegrationService = serviceProvider.GetService<IHupSpotIntegrationService>();

    }
}
