namespace BCT.Application.UseCases.Queries;
public class GetCompanyUsersUseCase : IGetCompanyUsersUseCase
{
    private readonly ICompanyRepository companyRepository;

    public GetCompanyUsersUseCase(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task<User[]> ExecuteAsync(Company company)
    {
        return (await companyRepository.GetCompanyUsers(company)).ToArray();
    }
}
