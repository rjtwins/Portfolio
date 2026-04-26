namespace BCT.Application.UseCases.Commands;
public interface IAddUserToCompanyUseCase : IUseCase
{
    Task ExecuteAsync(User user, Company company);
}