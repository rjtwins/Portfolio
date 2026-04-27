namespace BCT.Application.UseCases.Commands;
public interface IResetUserPasswordUseCase : IUseCase
{
    public Task ExecuteAsync(User user);
}
