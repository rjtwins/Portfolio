
namespace BCT.Application.UseCases.Commands;

public interface ISaveValueUseCase : IUseCase
{
    Task Execute(ValueModel value, string userId);
}