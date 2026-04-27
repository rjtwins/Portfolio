namespace BCT.Application.UseCases.Commands;
public interface IDeleteProjectUseCase : IUseCase
{
    Task ExecuteAsync(Project project, string userId);
}