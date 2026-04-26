
namespace BCT.Application.UseCases.Commands;

public interface ISaveProjectGridWizardUseCase : IUseCase
{
    Task<ProjectGridWizard?> ExecuteAsync(ProjectGridWizard? projectGridWizard);
}