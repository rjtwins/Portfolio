
namespace BCT.Application.UseCases.Queries;

public interface IGetProjectAdditionalCriteriaUseCase : IUseCase
{
    Task<List<StringValue>> Execute(Project p);
}