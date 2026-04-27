namespace BCT.Application.UseCases.Commands;

public interface IAddRoleToUserUseCase : IUseCase
{
    Task ExecuteAsync(User user, Role role);
    Task ExecuteAsync(string userAuthId, string roleAuthId);
}
