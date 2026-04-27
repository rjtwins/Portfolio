namespace BCT.Application.UseCases.Commands;

public interface ICreateUserUseCase : IUseCase
{
    Task<string> ExecuteAsync(string email);
}