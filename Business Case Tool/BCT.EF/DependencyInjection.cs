using BCT.Application.ServiceInterfaces;
using BCT.Domain.Entities;
using BCT.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCT.EF;
public static class DepenencyInjection
{
    public static IServiceCollection AddEF(this IServiceCollection services, IConfiguration configuration)
    {
        //Concrete implementations of repositories
        services.AddSingleton<IRepository<Company>, Repository<Company>>();
        services.AddSingleton<ICompanyRepository, CompanyRepository>();
        services.AddSingleton<IRepository<User>, Repository<User>>();
        services.AddSingleton<IRepository<Project>, Repository<Project>>();
        services.AddSingleton<IRepository<DoubleValue>, Repository<DoubleValue>>();
        services.AddSingleton<IRepository<StringValue>, Repository<StringValue>>();
        services.AddSingleton<IRepository<BoolValue>, Repository<BoolValue>>();
        services.AddSingleton<IRepository<ProjectGridWizard>, Repository<ProjectGridWizard>>();
        services.AddSingleton<IRepository<Scenario>, Repository<Scenario>>();

        //services.AddSingleton<IRepository<Domain.Entities.Attribute>, Repository<Domain.Entities.Attribute>>();

        services.AddSingleton<ITagRepository, TagRepository>();

        //DBContext factory:
        var baseFolder = configuration["DB:BaseFolder"];
        baseFolder = string.IsNullOrEmpty(baseFolder) ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) : baseFolder;
        string dbPath = Path.Join(baseFolder, configuration["DB:DBName"]);

        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath};Cache=Shared;");
            options.UseSeeding((context, _) =>
            {

            });
        }, ServiceLifetime.Singleton);

        return services;
    }
}