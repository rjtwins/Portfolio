using BCT.Application.Services;
using BCT.Application.SharedLogic;

namespace BCT.Application.UseCases.Commands;
public class SaveProjectUseCase : ISaveProjectUseCase
{
    private readonly IRepository<Project> projectRepository;
    private readonly ProjectContentUpdatedNotifier projectContentUpdatedNotifier;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IUpdateProjectTime updateProjectTime;

    public SaveProjectUseCase(
        IRepository<Project> projectRepository, 
        ProjectContentUpdatedNotifier projectContentUpdatedNotifier,
        IRepository<DoubleValue> doubleValueRepository,
        IUpdateProjectTime updateProjectTime)
    {
        this.projectRepository = projectRepository;
        this.projectContentUpdatedNotifier = projectContentUpdatedNotifier;
        this.doubleValueRepository = doubleValueRepository;
        this.updateProjectTime = updateProjectTime;
    }

    public async Task<Project?> ExecuteAsync(Project p, string userId)
    {
        if (p == null)
            return null;

        var oldProject = await projectRepository.Get(p.Id);

        if (oldProject == null)
            return null;

        if (oldProject.StartYear != p.StartYear || oldProject.Horizon != p.Horizon)
            await updateProjectTime.ExecuteAsync(p);

        p = await projectRepository.Update(p);

        projectContentUpdatedNotifier.Notify(new(p, userId));

        return p;
    }
}
