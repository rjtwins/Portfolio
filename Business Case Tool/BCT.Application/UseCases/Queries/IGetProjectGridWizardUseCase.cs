
namespace BCT.Application.UseCases.Queries;

public interface IGetProjectGridWizardUseCase : IUseCase
{
    Task<ProjectGridWizard?> ExecuteAsync(int projectId);
}