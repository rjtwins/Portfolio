
namespace BCT.Application.UseCases.Commands;

public interface IApplyProjectGridWizardUseCase : IUseCase
{
    Task ExecuteAsync(ProjectGridWizard? projectGridWizard, string userId);
}