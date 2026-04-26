namespace BCT.Application.UseCases.Queries;
public class GetProjectsUseCase : IGetProjectsUseCase
{
    private readonly IRepository<Project> projectRepository;

    public GetProjectsUseCase(IRepository<Project> projectRepository)
    {
        this.projectRepository = projectRepository;
    }

    public async Task<List<Project>> ExecuteAsync()
    {
        return await projectRepository.GetAll();
    }
}
