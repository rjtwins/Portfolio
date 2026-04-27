namespace BCT.Application.UseCases.Queries;

public interface IGetAllRolesUseCase : IUseCase
{
    Task<Role[]> ExecuteAsync();
}
