using BCT.Application.Exceptions;
using BCT.Application.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using System.Diagnostics;

namespace BCT.Auth0Api;
public static class DepenencyInjection
{
    public static IServiceCollection AddAuth0Api(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAuthManagementService, Auth0ManagementApi>();
        services.AddSingleton<IAuth0ManagementTokenRefreshService, Auth0ManagementTokenRefreshService>();

        //Polly:
        services.AddResiliencePipeline("retry-Auth0API-call", builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions()
                {
                    ShouldHandle = new PredicateBuilder().Handle<AuthServiceToManyRequestsException>(),
                    Delay = TimeSpan.FromSeconds(2),
                    MaxRetryAttempts = 10,
                    OnRetry = static args =>
                    {
                        Console.WriteLine($"Retry on Auth0 api call attempt {args.AttemptNumber} of {10}");
                        Debug.WriteLine($"Retry on Auth0 api call attempt {args.AttemptNumber} of {10}");

                        return default;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(10));
        });

        return services;
    }   
}
