
namespace BCT.Application.UseCases.Queries;

public interface IGetProjectTagsUseCase : IUseCase
{
    Task<List<Tag>> ExecuteAsync(int projectId);
}