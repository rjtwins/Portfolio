using BCT.Application.UseCases;
namespace BCT.Application.SharedLogic;
public interface IUpdateProjectTime : IUseCase
{
    Task ExecuteAsync(Project p);
}
