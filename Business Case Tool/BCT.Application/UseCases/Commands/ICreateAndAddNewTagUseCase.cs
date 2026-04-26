
namespace BCT.Application.UseCases.Commands;

public interface ICreateAndAddNewTagUseCase : IUseCase
{
    Task<Tag> ExecuteAsync(string tagText, int projectId);
}