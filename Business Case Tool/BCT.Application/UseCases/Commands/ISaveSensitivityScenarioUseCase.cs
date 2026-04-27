
namespace BCT.Application.UseCases.Commands;

public interface ISaveSensitivityScenarioUseCase : IUseCase
{
    Task<Scenario?> ExecuteAsync(Scenario sensitivityScenario);
}