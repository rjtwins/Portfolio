namespace BCT.Application.UseCases.Queries;
public class GetProjectUseCase : IGetProjectUseCase
{
    private readonly IRepository<Project> repository;

    public GetProjectUseCase(IRepository<Project> repository)
    {
        this.repository = repository;
    }

    public async Task<Project?> ExecuteAsync(int projectId)
    {
        return await repository.Get(projectId);
    }
}
