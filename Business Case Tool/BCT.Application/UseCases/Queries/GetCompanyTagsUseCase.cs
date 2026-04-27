namespace BCT.Application.UseCases.Queries;
public class GetCompanyTagsUseCase : IGetCompanyTagsUseCase
{
    private ICompanyRepository companyRepository;

    public GetCompanyTagsUseCase(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    public async Task<List<Tag>> ExecuteAsync(int companyId)
    {
        var company = await companyRepository.Get(companyId);
        return await companyRepository.GetCompanyTags(companyId);
    }
}
