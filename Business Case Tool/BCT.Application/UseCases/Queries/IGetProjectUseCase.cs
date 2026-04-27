namespace BCT.Application.UseCases.Queries;
public interface IGetProjectUseCase : IUseCase
{
    Task<Project?> ExecuteAsync(int projectId);
}