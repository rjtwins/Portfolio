namespace BCT.Application.UseCases.Queries;
public class GetProjectGridWizardUseCase : IGetProjectGridWizardUseCase
{
    private readonly IRepository<ProjectGridWizard> projectGridWizardRepository;
    private readonly IRepository<Project> projectRepository;
    private readonly IRepository<DoubleValue> doubleValueRepository;

    public GetProjectGridWizardUseCase(IRepository<ProjectGridWizard> projectGridWizardRepository, IRepository<Project> projectRepository, IRepository<DoubleValue> doubleValueRepository)
    {
        this.projectGridWizardRepository = projectGridWizardRepository;
        this.projectRepository = projectRepository;
        this.doubleValueRepository = doubleValueRepository;
        this.projectGridWizardRepository = projectGridWizardRepository;
    }

    public async Task<ProjectGridWizard?> ExecuteAsync(int projectId)
    {
        if (projectId <= 0)
            return null;

        var projectGridWizard = await projectGridWizardRepository.FirstOrDefault(x => x.ProjectId == projectId);
        var project = await projectRepository.Get(projectId);

        if (project == null)
            return null;

        if (projectGridWizard == null)
            projectGridWizard = new();

        projectGridWizard.ProjectId = projectId;
        projectGridWizard.StartYear = project.StartYear;
        projectGridWizard.Horizon = project.Horizon;
        projectGridWizard.Interest = (await doubleValueRepository.FirstOrDefault(x => x.ProjectId == projectId && x.Key == "Interest"))?.Value ?? 0;
        Type type = typeof(ProjectGridWizard);

        foreach (var item in Configuration.Project.OverTimeValueWizardMap)
        {
            type.GetProperty(item.Value + "Year")?.SetValue(projectGridWizard, project.StartYear);
        }

        if (projectGridWizard.Id == 0)
            await projectGridWizardRepository.Add(projectGridWizard);
        else
            await projectGridWizardRepository.Update(projectGridWizard);

        return projectGridWizard;
    }
}
