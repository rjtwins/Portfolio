namespace BCT.Application.UseCases.Commands;
public interface ICreateProjectUseCase : IUseCase
{
    Task<Project> ExecuteAsync(Company company, string userId, string projectName = "");
}