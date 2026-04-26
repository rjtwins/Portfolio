namespace BCT.Application.UseCases.Queries;

public interface IGetUsersUseCase : IUseCase
{
    Task<User[]> ExecuteAsync();
}
