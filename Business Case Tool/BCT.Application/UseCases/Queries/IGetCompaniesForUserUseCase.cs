namespace BCT.Application.UseCases.Queries;
public interface IGetCompaniesForUserUseCase : IUseCase
{
    Task<List<Company>> ExecuteAsync(User user);
}