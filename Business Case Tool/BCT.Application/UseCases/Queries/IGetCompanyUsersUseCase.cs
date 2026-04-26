namespace BCT.Application.UseCases.Queries;
public interface IGetCompanyUsersUseCase : IUseCase
{
    Task<User[]> ExecuteAsync(Company company);
}