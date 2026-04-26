using BCT.Application.Services;

namespace BCT.Application.UseCases.Commands;
public class SaveValueUseCase : ISaveValueUseCase
{
    private readonly IRepository<StringValue> stringValueRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;
    private readonly IRepository<BoolValue> boolValueRepository;
    private readonly IRepository<Project> projectRepository;
    private readonly ProjectContentUpdatedNotifier projectContentUpdatedNotifier;

    public SaveValueUseCase(IRepository<StringValue> stringValueRepository, 
        IRepository<DoubleValue> doubleValueRepository, 
        IRepository<BoolValue> boolValueRepository,
        IRepository<Project> projectRepository,
        ProjectContentUpdatedNotifier projectContentUpdatedNotifier)
    {
        this.stringValueRepository = stringValueRepository;
        this.doubleValueRepository = doubleValueRepository;
        this.boolValueRepository = boolValueRepository;
        this.projectRepository = projectRepository;
        this.projectContentUpdatedNotifier = projectContentUpdatedNotifier;
    }

    public async Task Execute(ValueModel value, string userId)
    {
        var project = await projectRepository.Get(value.ProjectId);

        if (project == null)
            return;

        if (value is StringValue sv)
        {
            var old = await stringValueRepository.Get(sv.Id);
            if (old == null)
                return;
            old.Value = sv.Value;
            await stringValueRepository.Update(old);

            projectContentUpdatedNotifier.Notify(new(project, userId));
            return;
        }

        if (value is BoolValue bv)
        {
            var old = await boolValueRepository.Get(bv.Id);
            if (old == null)
                return;
            old.Value = bv.Value;
            await boolValueRepository.Update(old);
            projectContentUpdatedNotifier.Notify(new(project, userId));
            return;
        }

        if (value is DoubleValue dv)
        {
            var old = await doubleValueRepository.Get(dv.Id);
            if (old == null)
                return;
            old.Value = dv.Value;
            await doubleValueRepository.Update(old);
            projectContentUpdatedNotifier.Notify(new(project, userId));
            return;
        }
    }
}
