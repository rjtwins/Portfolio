namespace BCT.Application.UseCases.Commands;
public class AddUserToCompanyUseCase : IAddUserToCompanyUseCase
{
    private readonly ICompanyRepository companyRepository;

    public AddUserToCompanyUseCase(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task ExecuteAsync(User user, Company company)
    {
        await companyRepository.AddUserToCompany(user, company);
    }
}
