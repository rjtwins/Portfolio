namespace BCT.Application.UseCases.Queries;
public class GetProjectTagsUseCase : IGetProjectTagsUseCase
{
    private readonly ITagRepository tagRepository;
    public GetProjectTagsUseCase(ITagRepository tagRepository)
    {
        this.tagRepository = tagRepository;
    }

    public async Task<List<Tag>> ExecuteAsync(int projectId)
    {
        var tags = await tagRepository.GetProjectTags(projectId);
        return tags;
    }
}
