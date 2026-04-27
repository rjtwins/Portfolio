namespace BCT.Application.UseCases.Commands;
public class AddTagToProjectUseCase : IAddTagToProjectUseCase
{
    private readonly ITagRepository tagRepository;

    public AddTagToProjectUseCase(ITagRepository tagRepository)
    {
        this.tagRepository = tagRepository;
    }

    public async Task ExecuteAsync(int tagId, int projectId)
    {
        await tagRepository.AddTagToProject(projectId, tagId);
    }
}
