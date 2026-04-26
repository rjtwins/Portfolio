namespace BCT.Application.UseCases.Commands;
public class CreateAndAddNewTagUseCase : ICreateAndAddNewTagUseCase
{
    private readonly ITagRepository tagRepository;
    private readonly IRepository<Project> projectRepository;
    private readonly ICompanyRepository companyRepository;

    public CreateAndAddNewTagUseCase(ITagRepository tagRepository, IRepository<Project> projectRepository, ICompanyRepository companyRepository)
    {
        this.tagRepository = tagRepository;
        this.projectRepository = projectRepository;
        this.companyRepository = companyRepository;
    }

    public async Task<Tag> ExecuteAsync(string tagText, int projectId)
    {
        var project = await projectRepository.Get(projectId);

        if (project == null)
            throw new InvalidOperationException($"Project with id {projectId} could not be found.");

        var company = await companyRepository.FirstOrDefault(x => x.Id == project.CompanyId);

        if (company == null)
            throw new InvalidOperationException($"Company with id {project.CompanyId} could not be found.");

        return await companyRepository.AddTagToCompany(tagText, company.Id, projectId);
    }
}
