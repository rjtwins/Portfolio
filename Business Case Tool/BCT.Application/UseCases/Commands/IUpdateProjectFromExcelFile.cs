namespace BCT.Application.UseCases.Commands;

public interface IUpdateProjectFromExcelFile : IUseCase
{
    Task ExecuteAsync(Project project, string excel);
}
