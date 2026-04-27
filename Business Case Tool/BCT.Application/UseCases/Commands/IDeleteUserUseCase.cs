namespace BCT.Application.UseCases.Commands;
public interface IDeleteUserUseCase : IUseCase
{
    public Task ExecuteAsync(User user);
}
