
namespace BCT.Application.UseCases.Queries;

public interface IGetProjectValuesUseCase : IUseCase
{
    Task<List<ValueModel>> Execute(int projectId);
}