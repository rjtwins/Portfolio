namespace BCT.Application.UseCases.Commands;
public class RemoveUserFromCompanyUseCase : IRemoveUserFromCompanyUseCase
{
    private readonly ICompanyRepository companyRepository;

    public RemoveUserFromCompanyUseCase(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task ExecuteAsync(User user, Company company)
    {
        await companyRepository.RemoveUserFromCompany(user, company);
    }
}
