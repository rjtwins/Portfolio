namespace BCT.Application.UseCases.Commands;
public class RemoveTagFromProjectUseCase : IRemoveTagFromProjectUseCase
{
    private readonly ITagRepository tagRepository;
    private readonly IRepository<Project> projectRepository;
    private readonly ICompanyRepository companyRepository;

    public RemoveTagFromProjectUseCase(
        ITagRepository tagRepository,
        IRepository<Project> projectRepository,
        ICompanyRepository companyRepository)
    {
        this.tagRepository = tagRepository;
        this.projectRepository = projectRepository;
        this.companyRepository = companyRepository;
    }

    public async Task ExecuteAsync(int tagId, int projectId)
    {
        await tagRepository.RemoveTagFromProject(projectId, tagId);

        var tag = await tagRepository.Get(tagId);

        //Delete if no more project references.
        //Do not delete if configured global tag.
        if (tag.Projects.Count == 0 && !Configuration.Project.GlobalTags.Contains(tag.Text))
        {
            await tagRepository.Delete(tag);
        }
    }
}
