
namespace BCT.Application.UseCases.Commands;

public interface ISaveCompanyUseCase : IUseCase
{
    Task<Company> ExecuteAsync(Company company, string userId);
}