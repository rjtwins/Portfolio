
namespace BCT.Application.UseCases.Queries;

public interface IGetCompanyTagsUseCase : IUseCase
{
    Task<List<Tag>> ExecuteAsync(int companyId);
}