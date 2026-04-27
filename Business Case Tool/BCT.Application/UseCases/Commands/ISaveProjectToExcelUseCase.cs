namespace BCT.Application.UseCases.Commands;

public interface ISaveProjectToExcelUseCase : IUseCase
{
    Task<string> ExecuteAsync(Project project);
}
