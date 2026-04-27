namespace BCT.Application.UseCases.Queries;

public interface IGetProjectDoubleValues : IUseCase
{
    Task<List<DoubleValue>> ExecuteAsync(Project project);
}