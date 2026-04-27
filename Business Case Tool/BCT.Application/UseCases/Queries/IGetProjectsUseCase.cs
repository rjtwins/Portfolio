namespace BCT.Application.UseCases.Queries;
public interface IGetProjectsUseCase : IUseCase
{
    Task<List<Project>> ExecuteAsync();
}