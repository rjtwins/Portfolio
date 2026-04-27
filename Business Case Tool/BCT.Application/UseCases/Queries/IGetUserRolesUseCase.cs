namespace BCT.Application.UseCases.Queries;

public interface IGetUserRolesUseCase : IUseCase
{
    Task<Role[]> ExecuteAsync(string auth0Id);
    Task<Role[]> ExecuteAsync(int userId);
    Task<Role[]> ExecuteAsync(User user);
}