namespace BCT.Application.UseCases.Commands;
public class SaveSensitivityScenarioUseCase : ISaveSensitivityScenarioUseCase
{
    private readonly IRepository<Scenario> sensitivityScenarioRepository;

    public SaveSensitivityScenarioUseCase(IRepository<Scenario> sensitivityScenarioRepository)
    {
        this.sensitivityScenarioRepository = sensitivityScenarioRepository;
    }

    public async Task<Scenario?> ExecuteAsync(Scenario sensitivityScenario)
    {
        if (sensitivityScenario == null)
            return null;

        if (sensitivityScenario.Id <= 0)
        {
            var newScenario = await sensitivityScenarioRepository.Add(sensitivityScenario);
            return newScenario;
        }
        else
        {
            var updatedScenario = await sensitivityScenarioRepository.Update(sensitivityScenario);
            return updatedScenario;
        }
    }
}
