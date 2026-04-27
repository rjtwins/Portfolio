
namespace BCT.Application.UseCases.Queries;

public interface IGetScenarioResultsUseCase : IUseCase
{
    Task<List<DoubleValue>> ExecuteAsync(Project project, Scenario s);
}