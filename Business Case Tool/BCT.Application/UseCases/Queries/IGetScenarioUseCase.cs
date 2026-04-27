
namespace BCT.Application.UseCases.Queries;

public interface IGetScenarioUseCase : IUseCase
{
    Task<Scenario?> ExecuteAsync(int projectId);
}