using BCT.Application.ServiceInterfaces;
using BCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BCT.EF.Repositories;
public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
    {

    }

    public async Task AddTagToProject(int projectId, int tagId)
    {
        using var context = GetContext();

        var project = await context.Projects.AsTracking().Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == projectId);
        var tag = await context.Tags.AsTracking().FirstOrDefaultAsync(x => x.Id == tagId);

        if(project == null || tag == null)
        {
            return;
        }

        project.Tags.Add(tag);
        tag.Projects.Add(project);

        context.Projects.Update(project);
        context.Tags.Update(tag);

        context.SaveChanges();
    }

    public async Task<List<Tag>> GetProjectTags(int projectId)
    {
        using var context = GetContext();

        var project = await context.Projects.Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == projectId);
        if (project == null)
        {
            return new();
        }

        return project.Tags.ToList();
    }

    public async Task RemoveTagFromProject(int projectId, int tagId)
    {
        using var context = GetContext();

        var project = await context.Projects.Include(x => x.Tags).AsTracking().FirstOrDefaultAsync(x => x.Id == projectId);
        var tag = await context.Tags.Include(x => x.Projects).AsTracking().FirstOrDefaultAsync(x => x.Id == tagId);

        if (project == null || tag == null)
            return;

        project.Tags.RemoveAll(x => x.Id == tagId);
        tag.Projects.RemoveAll(x => x.Id == projectId);

        context.Projects.Update(project);
        context.Tags.Update(tag);

        context.SaveChanges();
    }
}
