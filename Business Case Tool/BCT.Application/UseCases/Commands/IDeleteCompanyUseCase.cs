namespace BCT.Application.UseCases.Commands;
public interface IDeleteCompanyUseCase : IUseCase
{
    Task ExecuteAsync(Company company, string userId);
}