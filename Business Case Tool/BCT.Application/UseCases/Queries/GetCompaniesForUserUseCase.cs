namespace BCT.Application.UseCases.Queries;
public class GetCompaniesForUserUseCase : IGetCompaniesForUserUseCase
{
    private readonly IRepository<Company> companyRepository;

    public GetCompaniesForUserUseCase(IRepository<Company> companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task<List<Company>> ExecuteAsync(User user)
    {
        return await companyRepository.GetAll(c => c.Users.Any(y => y.Id == user.Id));
    }
}
