namespace BCT.Application.UseCases.Commands;

public interface IUpdateLastSelectedUseCase : IUseCase
{
    Task Execute(int userId, int? projectId, int? companyId);
}