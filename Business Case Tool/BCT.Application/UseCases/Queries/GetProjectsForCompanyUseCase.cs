namespace BCT.Application.UseCases.Queries;
public class GetProjectsForCompanyUseCase : IGetProjectsForCompanyUseCase
{
    private readonly IRepository<Project> projectRepository;

    public GetProjectsForCompanyUseCase(IRepository<Project> projectRepository)
    {
        this.projectRepository = projectRepository;
    }

    public async Task<List<Project>> ExecuteAsync(Company company)
    {
        if (company == null)
            return new();

        return await projectRepository.GetAll(x => x.CompanyId == company.Id);
    }
}
