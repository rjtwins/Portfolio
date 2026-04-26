namespace BCT.Application.UseCases.Commands;

public interface IRemoveRoleFromUserUseCase : IUseCase
{
	Task ExecuteAsync(User user, Role rol);
    Task ExecuteAsync(string userAuth0Id, string roleAuth0Id);
}
