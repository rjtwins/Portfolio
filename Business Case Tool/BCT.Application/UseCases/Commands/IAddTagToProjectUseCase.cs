
namespace BCT.Application.UseCases.Commands;

public interface IAddTagToProjectUseCase : IUseCase
{
    Task ExecuteAsync(int tagId, int projectId);
}