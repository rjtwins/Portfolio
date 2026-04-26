namespace BCT.Application.UseCases.Commands;
public interface IRemoveUserFromCompanyUseCase : IUseCase
{
    Task ExecuteAsync(User user, Company company);
}