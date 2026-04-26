using BCT.Application.Services;

namespace BCT.Application.UseCases.Commands;
public class DeleteProjectUseCase : IDeleteProjectUseCase
{
    private readonly IRepository<Project> projectRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IRepository<StringValue> stringValueRepository;
    private readonly IRepository<BoolValue> boolValueRepository;
    private readonly ProjectRemovedNotifier projectRemovedNotifier;

    public DeleteProjectUseCase(IRepository<Project> projectRepository,
        IRepository<DoubleValue> doubleValueRepository,
        IRepository<StringValue> stringValueRepository,
        IRepository<BoolValue> boolValueRepository,
        ProjectRemovedNotifier projectRemovedNotifier)
    {
        this.projectRepository = projectRepository;
        this.doubleValueRepository = doubleValueRepository;
        this.stringValueRepository = stringValueRepository;
        this.boolValueRepository = boolValueRepository;
        this.projectRemovedNotifier = projectRemovedNotifier;
    }

    public async Task ExecuteAsync(Project project, string userId)
    {
        (await stringValueRepository.GetAll(x => x.ProjectId == project.Id))
            .ForEach(x => stringValueRepository.Delete(x));
        (await boolValueRepository.GetAll(x => x.ProjectId == project.Id))
            .ForEach(x => boolValueRepository.Delete(x));
        (await doubleValueRepository.GetAll(x => x.ProjectId == project.Id))
            .ForEach(x => doubleValueRepository.Delete(x));

        await projectRepository.Delete(project);

        projectRemovedNotifier.Notify(new(project.Id, userId));
    }
}
