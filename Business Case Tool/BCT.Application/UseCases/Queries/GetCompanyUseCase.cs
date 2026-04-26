namespace BCT.Application.UseCases.Queries;
public class GetCompanyUseCase
{
    private readonly IRepository<Company> companyRepository;

    public GetCompanyUseCase(IRepository<Company> companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task<Company> ExecuteAsync(int id)
    {
        return await companyRepository.Get(id);
    }
}
