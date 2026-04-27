
namespace BCT.Application.UseCases.Commands;

public interface IRemoveTagFromProjectUseCase : IUseCase
{
    Task ExecuteAsync(int tagId, int projectId);
}