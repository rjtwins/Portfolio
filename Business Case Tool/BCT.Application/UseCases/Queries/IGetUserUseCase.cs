namespace BCT.Application.UseCases.Queries;
public interface IGetUserUseCase : IUseCase
{
    Task<User> ExecuteAsync(string authId);
}