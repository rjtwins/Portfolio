namespace BCT.Application.UseCases.Commands;
public class SaveProjectGridWizardUseCase : ISaveProjectGridWizardUseCase
{
    private readonly IRepository<ProjectGridWizard> projectGridWizardRepository;

    public SaveProjectGridWizardUseCase(IRepository<ProjectGridWizard> projectGridWizardRepository)
    {
        this.projectGridWizardRepository = projectGridWizardRepository;
    }

    public async Task<ProjectGridWizard?> ExecuteAsync(ProjectGridWizard? projectGridWizard)
    {
        if (projectGridWizard == null)
            return null;

        if (projectGridWizard.Id < 1)
        {
            await projectGridWizardRepository.Add(projectGridWizard);
        }
        else
        {
            await projectGridWizardRepository.Update(projectGridWizard);
        }

        return await projectGridWizardRepository.Get(projectGridWizard.Id);
    }
}
