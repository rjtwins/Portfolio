namespace BCT.Application.ServiceInterfaces;
public interface ITagRepository : IRepository<Tag>
{
    Task<List<Tag>> GetProjectTags(int projectId);
    Task AddTagToProject(int projectId, int tagId);

    Task RemoveTagFromProject(int projectId, int tagId);
}