using BCT.Application.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BCT.ExcelConverter;

public static class DependencyInjection
{
	public static IServiceCollection AddExcelConverter(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton<IExcelConverter, ExcelConverter>();
		return services;
	}
}
