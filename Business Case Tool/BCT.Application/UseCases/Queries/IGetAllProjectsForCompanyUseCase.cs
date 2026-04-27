namespace BCT.Application.UseCases.Queries;
public interface IGetProjectsForCompanyUseCase : IUseCase
{
    Task<List<Project>> ExecuteAsync(Company company);
}