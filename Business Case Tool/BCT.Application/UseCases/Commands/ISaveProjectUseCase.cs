namespace BCT.Application.UseCases.Commands;
public interface ISaveProjectUseCase : IUseCase
{
    Task<Project?> ExecuteAsync(Project p, string userId);
}