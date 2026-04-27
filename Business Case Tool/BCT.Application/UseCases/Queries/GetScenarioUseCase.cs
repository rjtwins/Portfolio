namespace BCT.Application.UseCases.Queries;
public class GetScenarioUseCase : IGetScenarioUseCase
{
    private readonly IRepository<Scenario> sensitivityScenarioRepository;

    public GetScenarioUseCase(IRepository<Scenario> sensitivityScenarioRepository)
    {
        this.sensitivityScenarioRepository = sensitivityScenarioRepository;
    }

    public async Task<Scenario?> ExecuteAsync(int projectId)
    {
        if (projectId <= 0)
            return null;

        var scenario = await sensitivityScenarioRepository.FirstOrDefault(x => x.ProjectId == projectId);

        if (scenario == null)
            return new Scenario()
            {
                ProjectId = projectId,
            };

        return scenario;
    }
}
