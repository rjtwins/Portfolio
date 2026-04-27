
namespace BCT.Application.UseCases.Queries;

public interface ICheckIfUserExistsUseCase : IUseCase
{
    Task<bool> ExecuteAsync(string authId);
}