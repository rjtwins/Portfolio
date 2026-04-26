
namespace BCT.Application.UseCases.Commands;

public interface IProcessMissingValueMigrationsUseCase : IUseCase
{
    Task Execute();
}