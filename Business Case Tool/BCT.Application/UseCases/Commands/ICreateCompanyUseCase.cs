namespace BCT.Application.UseCases.Commands;
public interface ICreateCompanyUseCase : IUseCase
{
    Task<Company> ExecuteAsync(User creator, string companyName, string userId);
}